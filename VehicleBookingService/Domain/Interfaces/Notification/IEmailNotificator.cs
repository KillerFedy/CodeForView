using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces.Notification
{
    public interface IEmailNotificator
    {
        Task SendUserVerificationAsync(string email, string verificationCode);
    }
}
