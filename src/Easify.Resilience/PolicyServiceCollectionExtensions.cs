﻿// This software is part of the Easify framework
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
using Microsoft.Extensions.DependencyInjection;
using Polly.Registry;

namespace Easify.Resilience
{
    public static class PolicyServiceCollectionExtensions
    {
        public static IServiceCollection AddPolicySupport(this IServiceCollection services, Action<PolicyRegistry> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null) throw new ArgumentNullException(nameof(configure));
            
            var registry = new PolicyRegistry();
            configure(registry);

            services.AddSingleton<IReadOnlyPolicyRegistry<string>>(registry);
            return services;
        } 
    }
}