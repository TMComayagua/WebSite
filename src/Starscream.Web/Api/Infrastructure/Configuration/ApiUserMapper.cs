using System;
using Starscream.Data;
using Starscream.Domain;
using Starscream.Domain.Entities;
using Starscream.Domain.Exceptions;
using Starscream.Domain.Services;
using Starscream.Web.Api.Infrastructure.Authentication;
using Nancy.Security;

namespace Starscream.Web.Api.Infrastructure.Configuration
{
    public class ApiUserMapper : IApiUserMapper<Guid>
    {
        readonly IReadOnlyRepository _readOnlyRepo;
        readonly ITimeProvider _timeProvider;

        public ApiUserMapper(IReadOnlyRepository readOnlyRepo, ITimeProvider timeProvider)
        {
            _readOnlyRepo = readOnlyRepo;
            _timeProvider = timeProvider;
        }

        #region IApiUserMapper<Guid> Members

        public IUserIdentity GetUserFromAccessToken(Guid token)
        {
            UserLoginSession userLoginSession = GetUserSessionFromToken(token);
            MakeSureTokenHasntExpiredYet(userLoginSession);
            return new LoggedInUserIdentity(userLoginSession);
        }

        #endregion

        UserLoginSession GetUserSessionFromToken(Guid token)
        {
            UserLoginSession userLoginSession;
            try
            {
                userLoginSession = _readOnlyRepo.First<UserLoginSession>(x => x.Id == token);
            }
            catch (ItemNotFoundException<UserLoginSession> e)
            {
                throw new TokenDoesNotExistException();
            }
            return userLoginSession;
        }

        void MakeSureTokenHasntExpiredYet(UserLoginSession userLoginSession)
        {
            DateTime expires = userLoginSession.Expires;
            DateTime now = _timeProvider.Now();
            if (expires < now)
            {
                throw new TokenExpiredException();
            }
        }
    }
}