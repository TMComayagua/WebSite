﻿using System;
using Nancy.Security;
using Starscream.Data;
using Starscream.Domain;
using Starscream.Domain.Entities;
using Starscream.Domain.Exceptions;
using Starscream.Domain.Services;
using Starscream.Domain.ValueObjects;
using Starscream.Web.Api.Infrastructure.Authentication;
using Starscream.Web.Api.Infrastructure.Exceptions;
using Starscream.Web.Api.Requests;
using Starscream.Web.Api.Responses;
using Nancy;
using Nancy.ModelBinding;

namespace Starscream.Web.Api.Modules
{
    public class LoginModule : NancyModule
    {
        public LoginModule(IPasswordEncryptor passwordEncryptor, IReadOnlyRepository readOnlyRepository,
                           IUserSessionFactory userSessionFactory, IMenuProvider menuProvider)
        {
            Post["/login"] =
                _ =>
                    {

                        var loginInfo = this.Bind<LoginRequest>();
                        if (loginInfo.Email == null) throw new UserInputPropertyMissingException("Email");
                        if (loginInfo.Password == null) throw new UserInputPropertyMissingException("Password");

                        EncryptedPassword encryptedPassword = passwordEncryptor.Encrypt(loginInfo.Password);

                        try
                        {
                            var user =
                                readOnlyRepository.First<UserEmailLogin>(
                                    x => x.Email == loginInfo.Email && x.EncryptedPassword == encryptedPassword.Password);

                            if (!user.IsActive) throw new DisableUserAccountException();
                            UserLoginSession userLoginSession = userSessionFactory.Create(user);

                            return new SuccessfulLoginResponse<Guid>(userLoginSession.Id, user.Name,
                                                                     userLoginSession.Expires, menuProvider.getFeatures(userLoginSession.GetClaimsAsArray()));
                        }
                        catch (ItemNotFoundException<UserEmailLogin>)
                        {
                            throw new UnauthorizedAccessException("Invalid email address or password. Please try again.");
                        }
                        catch (DisableUserAccountException)
                        {
                            throw new UnauthorizedAccessException("Your account has been disabled. Please contact your administrator for help.");
                        }
                    };

            Post["/login/facebook"] = _ =>
                                      {
                                          var loginInfo = this.Bind<LoginSocialRequest>();
                                          if (loginInfo.Email == null) throw new UserInputPropertyMissingException("Email");
                                          if (loginInfo.Id == null) throw new UserInputPropertyMissingException("Social Id");

                                          try
                                          {
                                              var user =
                                                  readOnlyRepository.First<UserFacebookLogin>(
                                                      x => x.Email == loginInfo.Email && x.FacebookId== loginInfo.Id );

                                              if (!user.IsActive) throw new DisableUserAccountException();

                                              UserLoginSession userLoginSession = userSessionFactory.Create(user);

                                              return new SuccessfulLoginResponse<Guid>(userLoginSession.Id, user.Name, userLoginSession.Expires, menuProvider.getFeatures(userLoginSession.GetClaimsAsArray()));
                                          }
                                          catch (ItemNotFoundException<UserEmailLogin>)
                                          {
                                              throw new UnauthorizedAccessException("Invalid facebook user, you need to register first.");
                                          }
                                          catch (DisableUserAccountException)
                                          {

                                              throw new UnauthorizedAccessException("Your account has been disabled. Please contact your administrator for help.");
                                          }
                                      };
            Get["/roles"] =
                _ =>
                {
                    this.RequiresAuthentication();
                   return Response.AsJson(menuProvider.getAllFeatures());
                };


            Post["/login/google"] = _ =>
            {
                var loginInfo = this.Bind<LoginSocialRequest>();
                if (loginInfo.Email == null) throw new UserInputPropertyMissingException("Email");
                if (loginInfo.Id == null) throw new UserInputPropertyMissingException("Social Id");

                try
                {
                    var user =
                        readOnlyRepository.First<UserGoogleLogin>(
                            x => x.Email == loginInfo.Email && x.GoogleId == loginInfo.Id);
                    
                    if (!user.IsActive) throw new DisableUserAccountException();

                    UserLoginSession userLoginSession = userSessionFactory.Create(user);

                    return new SuccessfulLoginResponse<Guid>(userLoginSession.Id, user.Name, userLoginSession.Expires, menuProvider.getFeatures(userLoginSession.GetClaimsAsArray()));
                }
                catch (ItemNotFoundException<UserEmailLogin>)
                {
                    throw new UnauthorizedAccessException("Invalid google user, you need to register first.");
                }
                catch (DisableUserAccountException)
                {
                    throw new UnauthorizedAccessException("Your account has been disabled. Please contact your administrator for help.");
                }


            };


        }
    }
}