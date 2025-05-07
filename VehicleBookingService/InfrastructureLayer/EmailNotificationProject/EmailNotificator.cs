using Domain.Interfaces.Notification;
using FluentEmail.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailNotificationProject.Common
{
    public class EmailNotificator(IFluentEmail fluentEmail) : IEmailNotificator
    {
        public async Task SendUserVerificationAsync(string email, string verificationCode)
        {
            await fluentEmail.To(email).Subject("VerificationCode")
                .Body(verificationCode)
                .SendAsync();    
        }
    }
}
