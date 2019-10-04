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

 using EasyApi.AspNetCore;
using EasyApi.Logging.SeriLog.Loggly;
using Microsoft.AspNetCore.Hosting;

namespace EasyApi.Sample.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebHost.Run<Startup>(s =>
            {
                if (s.Environment.IsDevelopment() || s.Environment.IsEnvironment("INT"))
                    return s.ConfigureLogger<Startup>();

                return s.ConfigureLogger<Startup>(c => c.UseLoggly(s.Configuration.GetSection("Logging:Loggly")));
            }, args);
        }
    }
}