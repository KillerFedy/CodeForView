using Domain.Common.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.DomainErrors
{
    internal static class DomainErrors
    {
        public static class User
        {
            public static readonly Error AlreadyRegistered = new Error(
                "User.AlreadyRegistered", "Can't register already registered user");

            public static readonly Error VerifyUserNotFound = new Error(
                 "User.VerifyUserNotFound", "Can't find verify user");

            public static readonly Error NotVerifyUserNotFound = new Error(
                "User.NotVerifyUserNotFound", "Can't find not verify user");

            public static readonly Error NotValidPassword = new Error(
                "User.NotValidPassword", "Password not valid");
        }

        public static class UserVerificationToken
        {
            public static readonly Error VerificationTokenNotFound = new Error(
                "UserVerificationToken.VerificationTokenNotFound", "Can't find user verification token");
            public static readonly Error VerificationTokenExpired = new Error(
                "UserVerificationToken.VerificationTokenNotFound", "User verification token expired");
        }

        public static class CountryLocation
        {
            public static readonly Error LocationNotFound = new Error(
                "CountryLocation.LocationNotFound", "Can't find country location");
        }
    }
}
