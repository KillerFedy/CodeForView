using AutoMapper;
using BusinessLogicCore.BusinessLogicModels;
using BusinessLogicCore.Infrastructure;
using Database;
using Database.Enums;
using Database.Model;
using Database.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelsProject.NewsDtos;

namespace BusinessLogicCore.Services;

public class NewsService(
    NewsRepository newsRepository,
    IUserService userService,
    NewsUserReadRepository newsUserReadRepository, IMapper mapper,
    IOptions<AppSettings> appSettings,
    ILogger<BaseService> logger, CurrentUserService currentUserService,
    ISignalRService signalRService) : BaseService(appSettings, logger), INewsService
{
    protected readonly IMapper _mapper = mapper;


    /// <summary>
    ///     Список новостей
    /// </summary>
    /// <returns></returns>
    public async Task<List<NewsDto>> All(bool isSeverity)
    {
        var filterPredicate = PredicateBuilder.MustBe<News>(x => !x.IsDeleted
                                                                 && x.IsSeverity == isSeverity
        );
        var currerntUserRole = currentUserService.GetCurrentUserRoleId();
        if (currerntUserRole != UserRoleEnum.PortalAdministrator)
            filterPredicate = filterPredicate.And(x => x.PublicationDate < DateTime.Now);
        var result = await newsRepository.GetNewsList(filterPredicate);
        var mappedResult = _mapper.Map<List<News>, List<NewsDto>>(result);
        if (isSeverity)
            foreach (var item in mappedResult)
            {
                var isNewsReadByCurrentUser = await IsNewsReadByCurrentUserAsync(item.Id);
                if (isNewsReadByCurrentUser) item.IsReadByCurrentUser = true;
            }

        return mappedResult;
    }


    /// <summary>
    ///     Удаление новости
    /// </summary>
    /// <param name="newsId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteNews(int newsId)
    {
        await newsRepository.SetIsDeletedAsync(newsId);
        return true;
    }

    /// <summary>
    ///     Статистика по прочитанным важным новостям
    /// </summary>
    /// <param name="newsId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<NewsUserReadDto>> GetByArticleAndUserId(int newsId, string userId)
    {
        var filter = PredicateBuilder.True<NewsUserRead>();
        if (newsId > 0) filter = filter.And(x => x.NewsId == newsId);
        if (userId != null && userId.Length > 0) filter = filter.And(x => x.UserId == userId);
        var result = await newsUserReadRepository.GetListWithNews(filter);
        var mappedResult = _mapper.Map<List<NewsUserRead>,
            List<NewsUserReadDto>>(result);
        foreach (var item in mappedResult)
        {
            var companyUserName = await userService.GetUserFullNameByOldId(item.UserId);
            item.CompanyUserName = companyUserName;
        }

        return mappedResult;
    }

    public async Task<NewsDto> GetById(int newsId)
    {
        var news = await newsRepository.GetItemAsync(newsId);
        if (news == null) throw new ArgumentException("News did not found");
        var mappedResult = _mapper.Map<News, NewsDto>(news);
        if (mappedResult.IsSeverity)
        {
            var isNewsReadByCurrentUser = await IsNewsReadByCurrentUserAsync(newsId);
            if (isNewsReadByCurrentUser) mappedResult.IsReadByCurrentUser = true;
        }

        return mappedResult;
    }

    /// <summary>
    ///     Список важных новостей, не прочитанных пользоватиелем
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<NewsShortDto>> GetUnreadNewsList(string userId)
    {
        var result = await newsRepository.GetListUnreadNewsAsync(userId);
        var mappedResult = _mapper.Map<List<News>, List<NewsShortDto>>(result);
        return mappedResult;
    }

    public async Task<bool> MarkNewsReadForCurrentUser(int newsId)
    {
        var currentAspNetUserId = currentUserService.GetCurrentAspNetUserId();
        var currentUserId = currentUserService.GetCurrentUserId();
        var newsUserRead = new NewsUserRead
        {
            NewsId = newsId,
            UserId = currentAspNetUserId,
            ReadDate = DateTime.Now
        };
        await newsUserReadRepository.InsertItemAsync(newsUserRead);
        var unreadCount = await newsRepository.GetCountUnreadNewsAsync(currentAspNetUserId);
        if (currentUserId.HasValue) await signalRService.UpdateNewsImportantCount(currentUserId.Value, unreadCount);
        return true;
    }

    public async Task<NewsDto> Post(NewsDto news)
    {
        var mappedData = _mapper.Map<NewsDto, News>(news);
        await newsRepository.InsertItemAsync(mappedData);

        await signalRService.NewsImportantAdd(1);
        var mappedResult = _mapper.Map<News, NewsDto>(mappedData);
        return mappedResult;
    }

    public async Task<NewsDto> Put(NewsDto news)
    {
        var mappedData = _mapper.Map<NewsDto, News>(news);
        await newsRepository.UpdateItemAsync(mappedData);
        var mappedResult = _mapper.Map<News, NewsDto>(mappedData);
        return mappedResult;
    }

    private async Task<bool> IsNewsReadByCurrentUserAsync(int newsId)
    {
        var filterPredicate = PredicateBuilder.MustBe<NewsUserRead>(x =>
            x.NewsId == newsId && x.UserId == currentUserService.GetCurrentAspNetUserId());
        var newsUserRead = await newsUserReadRepository.GetItemAsync(filterPredicate);
        return newsUserRead != null;
    }
}