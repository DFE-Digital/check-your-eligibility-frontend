using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Idioms;
using CheckYourEligibility_FrontEnd.Controllers;
using CheckYourEligibility_FrontEnd.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Claims;
using CheckYourEligibility_DfeSignIn.Models;
using Microsoft.AspNetCore.Mvc;




namespace CheckYourEligibility.TestBase
{
    [ExcludeFromCodeCoverage]
    public abstract class TestBase
    {

        public Fixture _fixture = new Fixture();

    }

}