using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogicCore.BusinessLogicModels;
using BusinessLogicCore.Infrastructure;
using Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelsProject.Base;
using ModelsProject.NewsDtos;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApiCore.Controllers;

[Route("api/news")]
[SwaggerTag("Работа с новостями")]
public class NewsController : BaseApiController
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService,
        IOptions<AppSettings> appSettings,
        ILogger<BaseApiController> logger) : base(appSettings, logger)
    {
        _newsService = newsService;
    }

    [HttpGet("stat/{newsId}")]
    [HttpGet("stat/{newsId}/{userId}")]
    public async Task<WebApiResult<List<NewsUserReadDto>>> FindNewsUserRead(int newsId,
        string userId)
    {
        try
        {
            Logger.LogInformation("SearchArticleReadMarks");
            var result = await _newsService.GetByArticleAndUserId(newsId, userId);
            return new WebApiResult<List<NewsUserReadDto>> { ResponseObject = result };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while searching ArticleReadMark");
            return new WebApiResult<List<NewsUserReadDto>>
            {
                ErrorType = ErrorTypes.NotFound,
                HasError = true,
                Message = "Ошибка при получении отметок о прочтении статьи"
            };
        }
    }

    [HttpGet]
    public async Task<WebApiResult<List<NewsDto>>> All([FromQuery] bool severity)
    {
        try
        {
            Logger.LogInformation("GetAllNews");
            var result = await _newsService.All(severity);
            return new WebApiResult<List<NewsDto>> { ResponseObject = result };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while getting all news");
            return new WebApiResult<List<NewsDto>>
            {
                ErrorType = ErrorTypes.NotFound,
                HasError = true,
                Message = "Не удалось получить новости"
            };
        }
    }

    [HttpGet("{id}")]
    public async Task<WebApiResult<NewsDto>> GetById(int id)
    {
        try
        {
            Logger.LogInformation("GetNewsById");
            var result = await _newsService.GetById(id);
            return new WebApiResult<NewsDto> { ResponseObject = result };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while getting news");
            return new WebApiResult<NewsDto>
            {
                ErrorType = ErrorTypes.NotFound,
                HasError = true,
                Message = "Не удалось получить новость"
            };
        }
    }

    [HttpPost]
    public async Task<WebApiResult<NewsDto>> PostNewsAsync([FromBody] NewsDto news)
    {
        try
        {
            Logger.LogInformation("PostNews");
            var result = await _newsService.Post(news);

            return new WebApiResult<NewsDto> { ResponseObject = result };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while post news");
            return new WebApiResult<NewsDto>
            {
                ErrorType = ErrorTypes.NotFound,
                HasError = true,
                Message = "Не удалось добавить новость"
            };
        }
    }

    [HttpPut]
    public async Task<WebApiResult<NewsDto>> PutNewsAsync([FromBody] NewsDto news)
    {
        try
        {
            Logger.LogInformation("PutNews");
            var result = await _newsService.Put(news);
            return new WebApiResult<NewsDto> { ResponseObject = result };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while putting news");
            return new WebApiResult<NewsDto>
            {
                ErrorType = ErrorTypes.NotFound,
                HasError = true,
                Message = "Не удалось обновить новость"
            };
        }
    }

    [HttpPut("markAsRead/{id}")]
    public async Task<WebApiResult<bool>> MarkNewsReadForCurrentUser(int id)
    {
        try
        {
            Logger.LogInformation("MarkNewsReadForCurrentUser");
            var result = await _newsService.MarkNewsReadForCurrentUser(id);
            return new WebApiResult<bool> { ResponseObject = result };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while marking news for current user");
            return new WebApiResult<bool>
            {
                ErrorType = ErrorTypes.Custom,
                HasError = true,
                Message = "Не удалось пометить новость для текущего пользователя"
            };
        }
    }

    [HttpDelete]
    public async Task<WebApiResult<bool>> DeleteNewsByIdAsync(int newsId)
    {
        try
        {
            Logger.LogInformation("DeleteNews");
            var result = await _newsService.DeleteNews(newsId);
            return new WebApiResult<bool> { ResponseObject = result };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while deleting news");
            return new WebApiResult<bool>
            {
                ErrorType = ErrorTypes.Custom,
                HasError = true,
                Message = "Не удалось удалить новость"
            };
        }
    }

    [HttpGet("unread-important/{userId}")]
    public async Task<WebApiResult<List<NewsShortDto>>> GetUnreadImportantNewsList(string userId)
    {
        try
        {
            Logger.LogInformation("GetUnreadImportantNewsList");
            var result = await _newsService.GetUnreadNewsList(userId);
            return new WebApiResult<List<NewsShortDto>> { ResponseObject = result };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while getting unread important news");
            return new WebApiResult<List<NewsShortDto>>
            {
                ErrorType = ErrorTypes.Custom,
                HasError = true,
                Message = "Не удалось получить непрочитанные важные новости"
            };
        }
    }
}