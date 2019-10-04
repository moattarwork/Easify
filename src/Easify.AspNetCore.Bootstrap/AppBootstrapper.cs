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
using System.Collections.Generic;
using EasyApi.AspNetCore.Cors;
using EasyApi.AspNetCore.Documentation;
using EasyApi.AspNetCore.ExceptionHandling;
using EasyApi.AspNetCore.Mvc;
using EasyApi.AspNetCore.RequestCorrelation;
using EasyApi.AspNetCore.RequestCorrelation.Core.OptionsBuilder;
using EasyApi.Bootstrap;
using EasyApi.Configurations.Fluents;
using EasyApi.ExceptionHandling;
using EasyApi.ExceptionHandling.ConfigurationBuilder;
using EasyApi.ExceptionHandling.ErrorBuilder.Fluent;
using EasyApi.Extensions;
using EasyApi.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyApi.AspNetCore.Bootstrap
{
    // TODO: Bootstrapping shouldn't be dependent to this but a bunch of options to middleware
    public sealed class AppBootstrapper<TStartup> :
        IBootstrapApplication,
        IConfigureContainer,
        IAddExtraConfigSection,
        IHandleAdditionalException,
        ISetDetailsLevel,
        IExtendPipeline,
        IConfigureRequestCorrelation,
        IConfigureApplicationBootstrapper where TStartup : class
    {
        private readonly IConfiguration _configuration;
        private readonly ConfigurationSectionBuilder _configurationSectionBuilder;
        private readonly GlobalErrorHandlerConfigurationBuilder _errorHandlerBuilder;

        private readonly List<Action<IServiceCollection, IConfiguration>> _pipelineExtenders =
            new List<Action<IServiceCollection, IConfiguration>>();

        private readonly IServiceCollection _services;
        private Func<IServiceCollection, IConfiguration, IServiceProvider> _containerFactory;

        private Func<IExcludeRequests, IBuildOptions> _requestCorrelationExtender = cop => cop.EnforceCorrelation();

        public AppBootstrapper(
            IServiceCollection services,
            IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _configurationSectionBuilder = new ConfigurationSectionBuilder(services, configuration);
            _errorHandlerBuilder = new GlobalErrorHandlerConfigurationBuilder(services);

            _errorHandlerBuilder.UseStandardMessage();
        }

        public IAddExtraConfigSection AndSection<TSection>()
            where TSection : class, new()
        {
            _configurationSectionBuilder.And<TSection>();
            return this;
        }

        public IAddExtraConfigSection AndSection<TSection>(string section)
            where TSection : class, new()
        {
            _configurationSectionBuilder.And<TSection>(section);
            return this;
        }

        public IHandleAdditionalException HandleApplicationException<
            TApplicationBaseException>()
            where TApplicationBaseException : Exception
        {
            _errorHandlerBuilder.Handle<TApplicationBaseException>();

            return this;
        }

        public IServiceProvider Bootstrap()
        {
            _configurationSectionBuilder.Build();

            _services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();
            _services.TryAddScoped<IUrlHelper, UrlHelper>();
            _services.TryAddScoped<IArgumentsFormatter, ArgumentsFormatter>();
            _services.TryAddSingleton(p => new ArgumentFormatterOptions());

            _services.AddHttpRequestContext();
            _services.AddGlobalExceptionHandler(ae => _errorHandlerBuilder.UseDefault());
            _services.AddRequestCorrelation(b => _requestCorrelationExtender(CorrelationOptionsBuilderExtensions.ExcludeDefaultUrls(b)));
            _services.AddDefaultMvc<TStartup>();
            _services.AddDefaultCorsPolicy();
            _services.AddDefaultApiDocumentation(_configuration);

            foreach (var extender in _pipelineExtenders) extender(_services, _configuration);

            return _containerFactory(_services, _configuration);
        }

        public IAddExtraConfigSection AddConfigSection<TSection>()
            where TSection : class, new()
        {
            _configurationSectionBuilder.AddSection<TSection>();
            return this;
        }

        public IAddExtraConfigSection AddConfigSection<TSection>(string section)
            where TSection : class, new()
        {
            _configurationSectionBuilder.AddSection<TSection>(section);
            return this;
        }

        // TODO: Need to be removed
        public IBootstrapApplication UseContainer<TContainer>(ContainerFactory<TContainer> containerFactory)
            where TContainer : class
        {
            if (containerFactory == null) throw new ArgumentNullException(nameof(containerFactory));

            _containerFactory = containerFactory.Create;
            return this;
        }

        public IConfigureRequestCorrelation UseStandardMessage()
        {
            _errorHandlerBuilder.UseStandardMessage();
            return this;
        }

        public IConfigureRequestCorrelation UseUserErrors()
        {
            _errorHandlerBuilder.UseUserErrors();
            return this;
        }

        public IConfigureRequestCorrelation UseDetailedErrors()
        {
            _errorHandlerBuilder.UseDetailedErrors();
            return this;
        }

        public IExtendPipeline ConfigureCorrelation(
            Func<IExcludeRequests, IBuildOptions> optionsProvider)
        {
            _requestCorrelationExtender = optionsProvider ??
                                          throw new ArgumentNullException(nameof(optionsProvider));
            return this;
        }

        public IExtendPipeline ConfigureCorrelation(Func<IExcludeRequests, ICorrelateRequests> optionsProvider)
        {
            if (optionsProvider == null) throw new ArgumentNullException(nameof(optionsProvider));
            _requestCorrelationExtender = r => optionsProvider(r).EnforceCorrelation();
            return this;
        }

        public IExtendPipeline Extend(Action<IServiceCollection, IConfiguration> pipelineExtender)
        {
            if (pipelineExtender == null) throw new ArgumentNullException(nameof(pipelineExtender));
            _pipelineExtenders.Add(pipelineExtender);
            return this;
        }

        public IHandleAdditionalException AndHandle<TThirdPartyBaseException>()
            where TThirdPartyBaseException : Exception
        {
            _errorHandlerBuilder.AndHandle<TThirdPartyBaseException>();
            return this;
        }

        public IHandleAdditionalException AndHandle<TThirdPartyBaseException>(
            Func<TThirdPartyBaseException, bool> predicate) where TThirdPartyBaseException : Exception
        {
            _errorHandlerBuilder.AndHandle(predicate);
            return this;
        }

        public IHandleAdditionalException AndHandle<TThirdPartyBaseException>(
            Func<ISetErrorBuilder<TThirdPartyBaseException>, IProvideErrorBuilder<TThirdPartyBaseException>>
                errorBuilderProvider) where TThirdPartyBaseException : Exception
        {
            _errorHandlerBuilder.AndHandle(errorBuilderProvider);
            return this;
        }

        public IHandleAdditionalException AndHandle<TThirdPartyBaseException>(
            Func<ISetErrorBuilder<TThirdPartyBaseException>, IProvideErrorBuilder<TThirdPartyBaseException>>
                errorBuilderProvider, Func<TThirdPartyBaseException, bool> predicate)
            where TThirdPartyBaseException : Exception
        {
            _errorHandlerBuilder.AndHandle(errorBuilderProvider, predicate);
            return this;
        }
    }
}