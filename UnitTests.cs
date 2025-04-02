using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeForView
{
    public class UnitTests
    {
        [Fact]
        public void CreateUser_ShouldOk()
        {

            _userRepository.Setup(repository => repository.GetByLogin(It.IsAny<string>()))
                        .Returns(() => null);

            var res = _userService.CreateUser(new User(default, "AnotherLogin", "TestUser", "88005553535", "Sharp", default));

            Assert.True(res.Success);
            Assert.Equal(string.Empty, res.Error);
        }



        [Fact]
        public void EditSchedule_ShouldOk()
        {
            Schedule schedule = new Schedule(default, new DateTime(1, 1, 1), new DateTime(1, 1, 1));

            _scheduleServiceMock.Setup(repository => repository.EditSchedule(schedule))
                .Returns(() => new Schedule(default, new DateTime(1, 1, 1), new DateTime(1, 1, 1)));

            var res = _scheduleService.EditSchedule(schedule);

            Assert.True(res.Success);
            Assert.Equal(string.Empty, res.Error);
        }
    }
}
