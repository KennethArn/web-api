using System.Linq;
using Xunit;
using Moq;
using GirafRest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using GirafRest.Controllers;
using System;
using Xunit.Abstractions;
using GirafRest.Test.Mocks;
using static GirafRest.Test.UnitTestExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using GirafRest.Models.DTOs;
using System.IO;

namespace GirafRest.Test
{
    public class PictogramControllerTest
    {
        private const int NEW_PICTOGRAM_ID = 400;
        private const int ADMIN_DEP_ONE = 0;
        private const int GUARDIAN_DEP_TWO = 1;
        private const int PUBLIC_PICTOGRAM = 0;
        private const int EXISTING_PICTOGRAM = 0;
        private const int ADMIN_PRIVATE_PICTOGRAM = 3;
        private const int DEP_ONE_PROTECTED_PICTOGRAM = 5;
        private const int NONEXISTING_PICTOGRAM = 999;
        private readonly string PNG_FILEPATH;
        private readonly string JPEG_FILEPATH;

        private TestContext _testContext;
        
        private readonly ITestOutputHelper _testLogger;

        public PictogramControllerTest(ITestOutputHelper output)
        {
            _testLogger = output;
            PNG_FILEPATH = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Mocks", "MockImage.png");
            JPEG_FILEPATH = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Mocks", "MockImage.jpeg");
        }

        private PictogramController initializeTest()
        {
            _testContext = new TestContext();

            var pc = new PictogramController(
                new MockGirafService(_testContext.MockDbContext.Object,
                _testContext.MockUserManager), _testContext.MockLoggerFactory.Object);
            _testContext.MockHttpContext = pc.MockHttpContext();

            return pc;
        }

        #region ReadPictogram(id)
        [Fact]
        public void ReadPictogram_NoLoginGetExistingPublic_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            
            var res = pc.ReadPictogram(PUBLIC_PICTOGRAM);
            IActionResult aRes = res.Result;

            Assert.IsType<OkObjectResult>(aRes);
        }

        [Fact]
        public void ReadPictogram_LoginGetExistingPublic_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            
            var res = pc.ReadPictogram(PUBLIC_PICTOGRAM);
            IActionResult aRes = res.Result;

            Assert.IsType<OkObjectResult>(aRes);
        }

        [Fact]
        public void ReadPictogram_NoLoginGetExistingPrivate_Unauthorized() {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();

            var res = pc.ReadPictogram(ADMIN_PRIVATE_PICTOGRAM);
            IActionResult aRes = res.Result;

            Assert.IsType<UnauthorizedResult>(aRes);
        }

        [Fact]
        public void ReadPictogram_NoLoginGetExistingProtected_Unauthorized() {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();

            var res = pc.ReadPictogram(DEP_ONE_PROTECTED_PICTOGRAM);
            IActionResult aRes = res.Result;

            Assert.IsType<UnauthorizedResult>(aRes);
        }

        [Fact]
        public void ReadPictogram_LoginGetOwnPrivate_Ok() {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var res = pc.ReadPictogram(ADMIN_PRIVATE_PICTOGRAM);
            IActionResult aRes = res.Result;

            Assert.IsType<OkObjectResult>(aRes);
        }

        [Fact]
        public void ReadPictogram_LoginGetProtectedInOwnDepartment_Ok() {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            var res = pc.ReadPictogram(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            if(res is ObjectResult)
            {
                var uRes = res as ObjectResult;
                _testLogger.WriteLine(uRes.Value.ToString());
            }

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void ReadPictogram_LoginGetProtectedInAnotherDepartment_Unauthorized() {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            var tRes = pc.ReadPictogram(DEP_ONE_PROTECTED_PICTOGRAM);
            var res = tRes.Result;

            if (res is ObjectResult)
            {
                var uRes = res as ObjectResult;
                _testLogger.WriteLine(uRes.Value.ToString());
            }

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void ReadPictogram_LoginGetExistingPrivateAnotherUser_Unauthorized() {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            var res = pc.ReadPictogram(ADMIN_PRIVATE_PICTOGRAM).Result;

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void ReadPictogram_LoginGetNonexistingPictogram_NotFound() {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var res = pc.ReadPictogram(NONEXISTING_PICTOGRAM);
            var pRes = res.Result;
            Assert.IsAssignableFrom<NotFoundResult>(pRes);
        }

        [Fact]
        public void ReadPictogram_NoLoginGetNonexistingPictogram_NotFound() {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();

            var res = pc.ReadPictogram(NONEXISTING_PICTOGRAM).Result;

            Assert.IsAssignableFrom<NotFoundResult>(res);
        }
        #endregion
        #region ReadPictograms()
        [Fact]
        public void ReadPictograms_NoLoginGetAll_Ok3Pictograms()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            _testContext.MockHttpContext.MockClearQueries();

            var res = pc.ReadPictograms().Result;
            var resList = convertToListAndLogTestOutput(res as OkObjectResult);

            Assert.True(3 == resList.Count);
        }

        [Fact]
        public void ReadPictograms_LoginGetAll_Ok5Pictograms()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockClearQueries();

            var res = pc.ReadPictograms().Result;
            var resList = convertToListAndLogTestOutput(res as OkObjectResult);

            Assert.True(5 == resList.Count);
        }

        [Fact]
        public void ReadPictograms_NoLoginGetAllWithValidQuery_Ok1Pictogram()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            _testContext.MockHttpContext.MockQuery("title", "picto1");

            var res = pc.ReadPictograms().Result;
            var resList = convertToListAndLogTestOutput(res as OkObjectResult);

            Assert.True(1 == resList.Count);
        }

        [Fact]
        public void ReadPictograms_NoLoginGetAllWithInvalidQuery_NotFound()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            _testContext.MockHttpContext.MockQuery("title", "invalid");

            var res = pc.ReadPictograms().Result;

            if (res is OkObjectResult)
            {
                convertToListAndLogTestOutput(res as OkObjectResult);
            }

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public void ReadPictograms_LoginGetAllWithValidQuery_Ok1Pictogram()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockQuery("title", "picto1");

            var res = pc.ReadPictograms().Result;

            var resList = convertToListAndLogTestOutput(res as OkObjectResult);

            Assert.True(1 == resList.Count);
        }

        [Fact]
        public void ReadPictograms_LoginGetAllWithInvalidQuery_NotFound()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            _testContext.MockHttpContext.MockQuery("title", "invalid");

            var res = pc.ReadPictograms().Result;

            if (res is OkObjectResult)
            {
                convertToListAndLogTestOutput(res as OkObjectResult);
            }
            if(res is BadRequestObjectResult)
            {
                _testLogger.WriteLine((res as BadRequestObjectResult).Value.ToString());
            }

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public void ReadPictograms_LoginGetAllWithValidQueryOnAnotherUsersPrivate_NotFound()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockQuery("title", "user 1");

            var res = pc.ReadPictograms().Result;

            if (res is OkObjectResult)
            {
                convertToListAndLogTestOutput(res as OkObjectResult);
            }

            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public void ReadPictograms_NoLoginGetAllWithValidQueryOnPrivate_NotFound()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            _testContext.MockHttpContext.MockQuery("title", "user 1");

            var res = pc.ReadPictograms().Result;

            if (res is OkObjectResult)
            {
                convertToListAndLogTestOutput(res as OkObjectResult);
            }

            Assert.IsType<NotFoundResult>(res);
        }        
        #endregion
        #region Create Pictogram
        private const string pictogramName = "TP";

        [Fact]
        public void CreatePictogram_LoginValidPublicDTO_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();

            var dto = new PictogramDTO()
            {
                AccessLevel = AccessLevel.PUBLIC,
                Title = "Public " + pictogramName,
                Id = NEW_PICTOGRAM_ID
            };

            var res = pc.CreatePictogram(dto).Result;
            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void CreatePictogram_LoginValidPrivateDTO_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);

            var dto = new PictogramDTO()
            {
                AccessLevel = AccessLevel.PRIVATE,
                Title = "Private " + pictogramName,
                Id = NEW_PICTOGRAM_ID
            };

            var res = pc.CreatePictogram(dto).Result;
            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void CreatePictogram_LoginValidProtectedDTO_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);

            var dto = new PictogramDTO()
            {
                AccessLevel = AccessLevel.PROTECTED,
                Title = "Protected " + pictogramName,
                Id = NEW_PICTOGRAM_ID
            };

            var res = pc.CreatePictogram(dto).Result;

            _testLogger.WriteLine(((res as OkObjectResult).Value as PictogramDTO).Id.ToString());

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void CreatePictogram_LoginInvalidDTO_BadRequest()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);

            PictogramDTO dto = null;

            var res = pc.CreatePictogram(dto).Result;

            if(res is BadRequestObjectResult)
            {
                _testLogger.WriteLine((res as BadRequestObjectResult).Value.ToString());
            }

            Assert.IsType<BadRequestObjectResult>(res);
        }
        #endregion
        #region UpdatePictogramInfo
        [Fact]
        public void UpdatePictogramInfo_NoLoginPrivate_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();

            var dto = new PictogramDTO()
            {
                Title = "Updated Pictogram",
                AccessLevel = AccessLevel.PRIVATE,
                Id = ADMIN_PRIVATE_PICTOGRAM
            };

            var res = pc.UpdatePictogramInfo(dto.Id, dto).Result;
            if(res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void UpdatePictogramInfo_NoLoginProtected_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();

            var dto = new PictogramDTO()
            {
                Title = "Updated Pictogram",
                AccessLevel = AccessLevel.PROTECTED,
                Id = DEP_ONE_PROTECTED_PICTOGRAM
            };

            var res = pc.UpdatePictogramInfo(dto.Id, dto).Result;

            if(res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void UpdatePictogramInfo_LoginPublic_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var dto = new PictogramDTO()
            {
                Title = "Updated Pictogram",
                AccessLevel = AccessLevel.PUBLIC,
                Id = PUBLIC_PICTOGRAM
            };

            var res = pc.UpdatePictogramInfo(dto.Id, dto).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void UpdatePictogramInfo_LoginOwnProtected_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var dto = new PictogramDTO()
            {
                Title = "Updated Pictogram",
                AccessLevel = AccessLevel.PROTECTED,
                Id = DEP_ONE_PROTECTED_PICTOGRAM
            };

            var res = pc.UpdatePictogramInfo(dto.Id, dto).Result;

            _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void UpdatePictogramInfo_LoginOwnPrivate_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var dto = new PictogramDTO()
            {
                Title = "Updated Pictogram",
                AccessLevel = AccessLevel.PRIVATE,
                Id = ADMIN_PRIVATE_PICTOGRAM
            };
            
            var res = pc.UpdatePictogramInfo(dto.Id, dto).Result;

            if(res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void UpdatePictogramInfo_LoginAnotherPrivate_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);

            var dto = new PictogramDTO()
            {
                Title = "Updated Pictogram",
                Id = ADMIN_PRIVATE_PICTOGRAM
            };

            var res = pc.UpdatePictogramInfo(dto.Id, dto).Result;

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void UpdatePictogramInfo_LoginAnotherProtected_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);

            var dto = new PictogramDTO()
            {
                Title = "Updated Pictogram",
                Id = DEP_ONE_PROTECTED_PICTOGRAM
            };

            var res = pc.UpdatePictogramInfo(dto.Id, dto).Result;

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void UpdatePictogramInfo_LoginNonexisting_NotFound()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);

            var dto = new PictogramDTO()
            {
                Title = "Updated Pictogram",
                Id = NONEXISTING_PICTOGRAM
            };

            var res = pc.UpdatePictogramInfo(dto.Id, dto).Result;

            if(res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsAssignableFrom<NotFoundResult>(res);
        }

        public void UpdatePictogramInfo_LoginInvalidDTO_BadRequest()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);

            PictogramDTO dto = null;

            var res = pc.UpdatePictogramInfo(PUBLIC_PICTOGRAM, dto).Result;

            _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<BadRequestObjectResult>(res);
        }
        #endregion
        #region DeletePictogram
        [Fact]
        public void DeletePictogram_NoLoginProtected_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();

            var res = pc.DeletePictogram(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void DeletePictogram_NoLoginPrivate_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();

            var res = pc.DeletePictogram(ADMIN_PRIVATE_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void DeletePictogram_LoginPublic_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var res = pc.DeletePictogram(PUBLIC_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<OkResult>(res);
        }

        [Fact]
        public void DeletePictogram_LoginOwnProtected_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var res = pc.DeletePictogram(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<OkResult>(res);
        }

        [Fact]
        public void DeletePictogram_LoginOwnPrivate_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var res = pc.DeletePictogram(ADMIN_PRIVATE_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<OkResult>(res);
        }

        [Fact]
        public void DeletePictogram_LoginAnotherProtected_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);

            var res = pc.DeletePictogram(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void DeletePictogram_LoginAnotherPrivate_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);

            var res = pc.DeletePictogram(ADMIN_PRIVATE_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void DeletePictogram_LoginNonexisting_NotFound()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var res = pc.DeletePictogram(NONEXISTING_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsAssignableFrom<NotFoundResult>(res);
        }
        #endregion
        #region CreateImage
        [Fact]
        public void CreateImage_NoLoginProtected_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            var res = pc.CreateImage(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void CreateImage_NoLoginPrivate_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            var res = pc.CreateImage(ADMIN_PRIVATE_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void CreateImage_LoginPublic_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            var res = pc.CreateImage(PUBLIC_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void CreateImage_LoginPrivate_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            var res = pc.CreateImage(ADMIN_PRIVATE_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<OkObjectResult>(res);
        }
        
        [Fact]
        public void CreateImage_LoginProtected_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            var res = pc.CreateImage(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void CreateImage_LoginAnotherProtected_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            var res = pc.CreateImage(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void CreateImage_LoginAnotherPrivate_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            var res = pc.CreateImage(ADMIN_PRIVATE_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void CreateImage_LoginNonexisting_NotFound()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            var res = pc.CreateImage(NONEXISTING_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsAssignableFrom<NotFoundResult>(res);
        }

        [Fact]
        public void CreateImage_LoginPublicNullBody_BadRequest()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestNoImage();

            var res = pc.CreateImage(PUBLIC_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<BadRequestObjectResult>(res);
        }

        [Fact]
        public void CreateImage_LoginPublicExistingImage_BadRequest()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            pc.CreateImage(EXISTING_PICTOGRAM);
            var res = pc.CreateImage(EXISTING_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<BadRequestObjectResult>(res);
        }

        [Fact]
        public void CreateImage_PublicJpeg_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            _testContext.MockHttpContext.MockRequestImage(JPEG_FILEPATH);

            var res = pc.CreateImage(PUBLIC_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<OkObjectResult>(res);
        }

        #endregion
        #region UpdatePictogramImage
        [Fact]
        public void UpdateImage_NoLoginProtected_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            pc.CreateImage(DEP_ONE_PROTECTED_PICTOGRAM);

            _testContext.MockUserManager.MockLogout();
            var res = pc.UpdatePictogramImage(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void UpdateImage_NoLoginPrivate_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            pc.CreateImage(ADMIN_PRIVATE_PICTOGRAM);

            _testContext.MockUserManager.MockLogout();
            var res = pc.UpdatePictogramImage(ADMIN_PRIVATE_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void UpdateImage_LoginPublic_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            pc.CreateImage(PUBLIC_PICTOGRAM);
            
            var res = pc.UpdatePictogramImage(PUBLIC_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<OkObjectResult>(res);
        }


        [Fact]
        public void UpdateImage_LoginPrivate_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            pc.CreateImage(ADMIN_PRIVATE_PICTOGRAM);

            var res = pc.UpdatePictogramImage(ADMIN_PRIVATE_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<OkObjectResult>(res);
        }


        [Fact]
        public void UpdateImage_LoginProtected_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            pc.CreateImage(DEP_ONE_PROTECTED_PICTOGRAM);

            var res = pc.UpdatePictogramImage(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<OkObjectResult>(res);
        }


        [Fact]
        public void UpdateImage_LoginAnotherPrivate_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            pc.CreateImage(ADMIN_PRIVATE_PICTOGRAM);

            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            var res = pc.UpdatePictogramImage(ADMIN_PRIVATE_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<UnauthorizedResult>(res);
        }


        [Fact]
        public void UpdateImage_LoginAnotherProtected_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            pc.CreateImage(DEP_ONE_PROTECTED_PICTOGRAM);

            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            var res = pc.UpdatePictogramImage(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<UnauthorizedResult>(res);
        }


        [Fact]
        public void UpdateImage_LoginNullBody_BadRequest()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            pc.CreateImage(PUBLIC_PICTOGRAM);

            _testContext.MockHttpContext.MockRequestNoImage();
            var res = pc.UpdatePictogramImage(PUBLIC_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<OkObjectResult>(res);
        }


        [Fact]
        public void UpdateImage_LoginNoImage_BadRequest()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);
            
            var res = pc.UpdatePictogramImage(ADMIN_PRIVATE_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<BadRequestObjectResult>(res);
        }


        [Fact]
        public void UpdateImage_LoginNonexisting_NotFound()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);
            
            var res = pc.UpdatePictogramImage(NONEXISTING_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<NotFoundResult>(res);
        }

        [Fact]
        public void UpdateImage_PublicJpegToJpeg_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            _testContext.MockHttpContext.MockRequestImage(JPEG_FILEPATH);

            pc.CreateImage(PUBLIC_PICTOGRAM);

            pc.UpdatePictogramImage(PUBLIC_PICTOGRAM);

            _testContext.MockHttpContext.MockRequestImage(JPEG_FILEPATH);

            var res = pc.UpdatePictogramImage(PUBLIC_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void UpdateImage_PublicPngToJpeg_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);

            pc.CreateImage(PUBLIC_PICTOGRAM);

            pc.UpdatePictogramImage(PUBLIC_PICTOGRAM);

            _testContext.MockHttpContext.MockRequestImage(JPEG_FILEPATH);

            var res = pc.UpdatePictogramImage(PUBLIC_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());
            Assert.IsType<OkObjectResult>(res);
        }

        #endregion
        #region ReadPictogramImage
        [Fact]
        public void ReadPictogramImage_NoLoginProtected_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);
            pc.CreateImage(DEP_ONE_PROTECTED_PICTOGRAM);

            _testContext.MockUserManager.MockLogout();
            var res = pc.ReadPictogramImage(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            Assert.IsType<UnauthorizedResult>(res);
        }
        
        [Fact]
        public void ReadPictogramImage_NoLoginPrivate_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);
            pc.CreateImage(ADMIN_PRIVATE_PICTOGRAM);

            _testContext.MockUserManager.MockLogout();
            var res = pc.ReadPictogramImage(ADMIN_PRIVATE_PICTOGRAM).Result;

            Assert.IsType<UnauthorizedResult>(res);
        }
        
        [Fact]
        public void ReadPictogramImage_LoginPublic_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);
            pc.CreateImage(PUBLIC_PICTOGRAM);
            
            var res = pc.ReadPictogramImage(PUBLIC_PICTOGRAM).Result;

            Assert.IsType<FileContentResult>(res);
        }
        
        [Fact]
        public void ReadPictogramImage_LoginProtected_Ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);
            pc.CreateImage(DEP_ONE_PROTECTED_PICTOGRAM);

            var res = pc.ReadPictogramImage(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            Assert.IsType<FileContentResult>(res);
        }
        
        [Fact]
        public void ReadPictogramImage_LoginAnotherPrivate_Unauthorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);
            pc.CreateImage(ADMIN_PRIVATE_PICTOGRAM);
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);

            var res = pc.ReadPictogramImage(ADMIN_PRIVATE_PICTOGRAM).Result;

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void ReadPictogramImage_LoginAnotherProtected_Unauhtorized()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            _testContext.MockHttpContext.MockRequestImage(PNG_FILEPATH);
            pc.CreateImage(DEP_ONE_PROTECTED_PICTOGRAM);
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);

            var res = pc.ReadPictogramImage(DEP_ONE_PROTECTED_PICTOGRAM).Result;

            Assert.IsType<UnauthorizedResult>(res);
        }

        [Fact]
        public void ReadPictogramImage_LoginPublicNoImage_NotFound()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var res = pc.ReadPictogramImage(PUBLIC_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<NotFoundObjectResult>(res);
        }

        [Fact]
        public void ReadPictogramImage_LoginNonexisting_NotFound()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            pc.CreateImage(NONEXISTING_PICTOGRAM);

            var res = pc.ReadPictogramImage(NONEXISTING_PICTOGRAM).Result;

            if (res is ObjectResult)
                _testLogger.WriteLine((res as ObjectResult).Value.ToString());

            Assert.IsType<NotFoundObjectResult>(res);
        }

        [Fact]
        public void ReadPictogramImage_GetPublicJpeg_ok()
        {
            var pc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            _testContext.MockHttpContext.MockRequestImage(JPEG_FILEPATH);
            pc.CreateImage(PUBLIC_PICTOGRAM);

            var res = pc.ReadPictogramImage(PUBLIC_PICTOGRAM).Result;

            Assert.IsType<FileContentResult>(res);
        }

        #endregion
        #region FilterByTitle
        [Theory]
        [InlineData("PUBLIC", 2)]
        [InlineData("", 7)]
        [InlineData("YYY", 0)]
        [InlineData("Pr", 4)]
        [InlineData("Pu", 2)]
        [InlineData("P", 6)]
        public void FilterByTitle(string query, int expectedPictograms) {
            var pc = initializeTest();
            
            var res = pc.FilterByTitle(_testContext.MockPictograms, query);

            Assert.Equal(expectedPictograms, res.Count);
        }
        #endregion
        #region Helpers
        private List<PictogramDTO> convertToListAndLogTestOutput(OkObjectResult result)
        {
            var list = result.Value as List<PictogramDTO>;
            list.ForEach(p => _testLogger.WriteLine(p.Title));

            return list;
        }
        #endregion
    }
}
