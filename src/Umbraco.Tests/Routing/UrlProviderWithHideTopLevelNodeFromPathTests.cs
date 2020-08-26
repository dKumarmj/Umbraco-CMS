﻿using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Infrastructure.Configuration;
using Umbraco.Tests.Common;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Testing;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class UrlProviderWithHideTopLevelNodeFromPathTests : BaseUrlProviderTest
    {
        private readonly GlobalSettings _globalSettings;

        public UrlProviderWithHideTopLevelNodeFromPathTests()
        {
            _globalSettings = new GlobalSettingsBuilder().WithHideTopLevelNodeFromPath(HideTopLevelNodeFromPath).Build();
        }

        protected override bool HideTopLevelNodeFromPath => true;

        protected override void ComposeSettings()
        {
            base.ComposeSettings();
            Composition.RegisterUnique(x => Microsoft.Extensions.Options.Options.Create(_globalSettings));
        }
        
        [TestCase(1046, "/")]
        [TestCase(1173, "/sub1/")]
        [TestCase(1174, "/sub1/sub2/")]
        [TestCase(1176, "/sub1/sub-3/")]
        [TestCase(1177, "/sub1/custom-sub-1/")]
        [TestCase(1178, "/sub1/custom-sub-2/")]
        [TestCase(1175, "/sub-2/")]
        [TestCase(1172, "/test-page/")] // not hidden because not first root
        public void Get_Url_Hiding_Top_Level(int nodeId, string niceUrlMatch)
        {
            var globalSettings = Mock.Get(Factory.GetInstance<IGlobalSettings>()); //this will modify the IGlobalSettings instance stored in the container
            globalSettings.Setup(x => x.HideTopLevelNodeFromPath).Returns(true);

            var requestHandlerSettings = new RequestHandlerSettingsBuilder().WithAddTrailingSlash(true).Build();
            var umbracoContext = GetUmbracoContext("/test", 1111, globalSettings: ConfigModelConversionsToLegacy.ConvertGlobalSettings(_globalSettings));
            var umbracoContextAccessor = new TestUmbracoContextAccessor(umbracoContext);
            var urlProvider = new DefaultUrlProvider(
                Microsoft.Extensions.Options.Options.Create(requestHandlerSettings),
                Logger,
                Microsoft.Extensions.Options.Options.Create(_globalSettings),
                new SiteDomainHelper(), umbracoContextAccessor, UriUtility);
            var publishedUrlProvider = GetPublishedUrlProvider(umbracoContext, urlProvider);

            var result = publishedUrlProvider.GetUrl(nodeId);
            Assert.AreEqual(niceUrlMatch, result);
        }
    }
}
