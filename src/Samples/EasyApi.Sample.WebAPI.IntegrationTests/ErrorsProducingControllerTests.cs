﻿using System;
using System.Net;
using System.Threading.Tasks;
using EasyApi.Sample.WebAPI.IntegrationTests.Helpers;
using EasyApi.Sample.WebAPI.IntegrationTests.Helpers.Domain;
using Newtonsoft.Json;
using Xunit;

namespace EasyApi.Sample.WebAPI.IntegrationTests
{
    public sealed class ErrorsProducingControllerTests : IDisposable
    {
        public ErrorsProducingControllerTests()
        {
            Fixture = TestServerFixture<StartupForIntegration>.Create("Integration");
        }

        public void Dispose()
        {
            try
            {
                Fixture.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private TestServerFixture<StartupForIntegration> Fixture { get; }

        [Fact]
        public async Task ShouldThrowFriendlyException()
        {
            // Arrange
            // Act
            var response = await Fixture.Client.GetAsync("api/Errors/throwFriendly");
            var responseString = await response.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<ErrorResponse>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("I am a user friendly exception message", content.Message);
            Assert.Collection(content.UserErrors, e1 =>
            {
                Assert.Equal("I am a user friendly exception message", e1.Message);
                Assert.Equal("OurApplicationException", e1.ErrorType);
                Assert.Empty(e1.ChildErrors);
            });
            Assert.Empty(content.RawErrors);
        }

        [Fact]
        public async Task ShouldThrowFriendlyHierarchyOfExceptions()
        {
            // Arrange
            // Act
            var response = await Fixture.Client.GetAsync("api/Errors/throwHierarchy");
            var responseString = await response.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<ErrorResponse>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Admin root thrown!", content.Message);
            Assert.Collection(content.UserErrors, e1 =>
            {
                Assert.Equal("Admin root thrown!", e1.Message);
                Assert.Equal("OurApplicationException", e1.ErrorType);
                Assert.Collection(e1.ChildErrors, t1 =>
                    {
                        Assert.Equal("My friedly leaf1!", t1.Message);
                        Assert.Equal("OurApplicationException", t1.ErrorType);
                        Assert.Empty(t1.ChildErrors);
                    },
                    t2 =>
                    {
                        Assert.Equal("Third party plugin has failed!", t2.Message);
                        Assert.Equal("ThirdPartyPluginFailedException", t2.ErrorType);
                        Assert.Empty(t2.ChildErrors);
                    }
                );
            });
            Assert.Empty(content.RawErrors);
        }

        [Fact]
        public async Task ShouldThrowThirdPartyException()
        {
            // Arrange
            // Act
            var response = await Fixture.Client.GetAsync("api/Errors/throwThirdParty");
            var responseString = await response.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<ErrorResponse>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Third party plugin has failed!", content.Message);
            Assert.Collection(content.UserErrors, e1 =>
            {
                Assert.Equal("Third party plugin has failed!", e1.Message);
                Assert.Equal("ThirdPartyPluginFailedException", e1.ErrorType);
                Assert.Empty(e1.ChildErrors);
            });
            Assert.Empty(content.RawErrors);
        }

        [Fact]
        public async Task ShouldThrowUnFriendlyException()
        {
            // Arrange
            // Act
            var response = await Fixture.Client.GetAsync("api/Errors/throwUnfriendly");
            var responseString = await response.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<ErrorResponse>(responseString);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("Unknown Error Happened", content.Message);
            Assert.Collection(content.UserErrors, e1 =>
            {
                Assert.Equal("Unknown Error Happened", e1.Message);
                Assert.Equal("UnknownException", e1.ErrorType);
                Assert.Empty(e1.ChildErrors);
            });
            Assert.Empty(content.RawErrors);
        }
    }
}