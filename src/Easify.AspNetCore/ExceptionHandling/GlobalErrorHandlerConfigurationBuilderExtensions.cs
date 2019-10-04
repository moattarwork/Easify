// This software is part of the Easify framework
// Copyright (C) 2019 Intermediate Capital Group
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using Easify.RestEase;
using EasyApi.AspNetCore.RequestCorrelation.Domain;
using EasyApi.ExceptionHandling;
using EasyApi.ExceptionHandling.ConfigurationBuilder;
using EasyApi.RestEase;
using FluentValidation;
using RestEase;

namespace EasyApi.AspNetCore.ExceptionHandling
{
    public static class GlobalErrorHandlerConfigurationBuilderExtensions
    {
        public const string GenericErrorMessage = "Unknown Error Happened";
        public const string GenericErrorType = "UnknownException";

        public static GlobalErrorHandlerConfigurationBuilder UseDefault(
            this GlobalErrorHandlerConfigurationBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder
                .AndHandle<ValidationException>()
                .AndHandle<NotCorrelatedRequestException>()
                .AndHandle<ApiException>(b => b.UseBuilderForApi(), f => f.ClientError())
                .UseGenericError(GenericErrorMessage, GenericErrorType);

            return builder;
        }
    }
}