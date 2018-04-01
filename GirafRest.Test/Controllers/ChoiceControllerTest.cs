using GirafRest.Controllers;
using GirafRest.Models;
using GirafRest.Models.DTOs;
using GirafRest.Test.Mocks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static GirafRest.Test.UnitTestExtensions;
using GirafRest.Models.Responses;

namespace GirafRest.Test
{
    public class ChoiceControllerTest
    {
        private TestContext _testContext;
        //private readonly ITestOutputHelper _outputHelpter;
        private const int PUBLIC_CHOICE = 0;
        private const int PRIVATE_CHOICE = 1;
        private const int PROTECTED_CHOICE = 2;
        private const int PRIVATE_PICTOGRAM = 3;
        private const int PROTECTED_PICTOGRAM = 5;
        private const int NONEXISTING = 999;
        private const int CREATE_CHOICE_ID = 100;
        private const int TWO = 2;
        private const int ADMIN_DEP_ONE = 0;
        private const int GUARDIAN_DEP_TWO = 1;

        public ChoiceControllerTest(/*ITestOutputHelper outputHelpter*/)
        {

        }

        private ChoiceController initializeTest()
        {
            _testContext = new TestContext();

            var cc = new ChoiceController(
                new MockGirafService(_testContext.MockDbContext.Object,
                _testContext.MockUserManager), _testContext.MockLoggerFactory.Object);
            _testContext.MockHttpContext = cc.MockHttpContext();

            return cc;
        }

        #region ReadChoice
        [Fact]
        public void ReadChoice_NoLoginGetPublic_OK()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            var res = cc.ReadChoice(_testContext.MockChoices[PUBLIC_CHOICE].Id).Result;

            Assert.IsType<Response<ChoiceDTO>>(res);
            Assert.True(res.Success);
            Assert.Equal(ErrorCode.NoError, res.ErrorCode);

            //Check data
            Assert.Equal(_testContext.MockChoices[PUBLIC_CHOICE].Title, res.Data.Title);
            Assert.Equal(_testContext.MockChoices[PUBLIC_CHOICE].LastEdit, res.Data.LastEdit);
            Assert.Equal(_testContext.MockChoices[PUBLIC_CHOICE].Id, res.Data.Id);
        }

        [Fact]
        public void ReadChoice_LoginGetPublic_OK()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var res = choiceController.ReadChoice(_testContext.MockChoices[PUBLIC_CHOICE].Id).Result;

            Assert.IsType<Response<ChoiceDTO>>(res);
            Assert.True(res.Success);
            Assert.Equal(ErrorCode.NoError, res.ErrorCode);

            //Check data
            Assert.Equal(_testContext.MockChoices[PUBLIC_CHOICE].Title, res.Data.Title);
            Assert.Equal(_testContext.MockChoices[PUBLIC_CHOICE].LastEdit, res.Data.LastEdit);
            Assert.Equal(_testContext.MockChoices[PUBLIC_CHOICE].Id, res.Data.Id);
        }

        [Fact]
        public void ReadChoice_NoLoginGetPrivate_Unauthorized()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            var res = cc.ReadChoice(_testContext.MockChoices[PRIVATE_CHOICE].Id).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }

        [Fact]
        public void ReadChoice_LoginGetPrivate_OK()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var res = choiceController.ReadChoice(_testContext.MockChoices[PRIVATE_CHOICE].Id).Result;

            Assert.IsType<Response<ChoiceDTO>>(res);
            Assert.True(res.Success);
            Assert.Equal(ErrorCode.NoError, res.ErrorCode);

            //Check data
            Assert.Equal(_testContext.MockChoices[PRIVATE_CHOICE].Title, res.Data.Title);
            Assert.Equal(_testContext.MockChoices[PRIVATE_CHOICE].LastEdit, res.Data.LastEdit);
            Assert.Equal(_testContext.MockChoices[PRIVATE_CHOICE].Id, res.Data.Id);
        }

        [Fact]
        public void ReadChoice_OtherLoginGetPrivate_Unauthorized()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            var res = choiceController.ReadChoice(_testContext.MockChoices[PRIVATE_CHOICE].Id).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }

        [Fact]
        public void ReadChoice_NoLoginGetProtected_Unauthorized()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            var res = cc.ReadChoice(_testContext.MockChoices[PROTECTED_CHOICE].Id).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }

        [Fact]
        public void ReadChoice_LoginGetProtected_OK()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var res = choiceController.ReadChoice(_testContext.MockChoices[PROTECTED_CHOICE].Id).Result;

            Assert.IsType<Response<ChoiceDTO>>(res);
            Assert.True(res.Success);
            Assert.Equal(ErrorCode.NoError, res.ErrorCode);

            //Check data
            Assert.Equal(_testContext.MockChoices[PROTECTED_CHOICE].Title, res.Data.Title);
            Assert.Equal(_testContext.MockChoices[PROTECTED_CHOICE].LastEdit, res.Data.LastEdit);
            Assert.Equal(_testContext.MockChoices[PROTECTED_CHOICE].Id, res.Data.Id);
        }

        [Fact]
        public void ReadChoice_OtherLoginGetProtected_Unauthorized()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            var res = choiceController.ReadChoice(_testContext.MockChoices[PROTECTED_CHOICE].Id).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }

        [Fact]
        public void ReadChoice_NoLoginGetNonExisting_NotFound()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            var res = cc.ReadChoice(NONEXISTING).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotFound, res.ErrorCode);
        }

        [Fact]
        public void ReadChoice_LoginGetNonExisting_NotFound()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);

            var res = choiceController.ReadChoice(NONEXISTING).Result;
            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotFound, res.ErrorCode);
        }
        #endregion

        #region CreateChoice
        [Fact]
        public void CreateChoice_NoLoginCreatePublic_Ok()
        {
            var title = "TestChoice";
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            List<Pictogram> options = _testContext.MockPictograms.Cast<Pictogram>().Where(p => p.AccessLevel == AccessLevel.PUBLIC).ToList();
            var res = cc.CreateChoice(new ChoiceDTO(new Choice(options, title) { Id = CREATE_CHOICE_ID })).Result;

            Assert.IsType<Response<ChoiceDTO>>(res);
            Assert.True(res.Success);
            Assert.Equal(ErrorCode.NoError, res.ErrorCode);

            // Check data
            Assert.Equal(title, res.Data.Title);

            foreach (var option in options)
            {
                Assert.True(res.Data.Options.Any(o => o.Id == option.Id));
            }
        }

        [Fact]
        public void CreateChoice_LoginCreatePublic_Ok()
        {
            var title = "TestChoice";
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            List<Pictogram> options = _testContext.MockPictograms
                .Cast<Pictogram>()
                .Where(p => p.AccessLevel == AccessLevel.PUBLIC)
                .ToList();
            var res = choiceController.CreateChoice(new ChoiceDTO(new Choice(options, title) { Id = CREATE_CHOICE_ID })).Result;

            Assert.IsType<Response<ChoiceDTO>>(res);
            Assert.True(res.Success);
            Assert.Equal(ErrorCode.NoError, res.ErrorCode);

            // Check data
            Assert.Equal(title, res.Data.Title);

            foreach (var option in options)
            {
                Assert.True(res.Data.Options.Any(o => o.Id == option.Id));
            }
        }

        [Fact]
        public void CreateChoice_NoLoginCreatePrivate_Unauthorized()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            List<Pictogram> options = new List<Pictogram> { _testContext.MockPictograms[PRIVATE_PICTOGRAM] };
            var res = cc.CreateChoice(new ChoiceDTO(new Choice(options, "TestChoice") { Id = CREATE_CHOICE_ID })).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }

        [Fact]
        public void CreateChoice_LoginCreatePrivate_Ok()
        {
            var title = "TestChoice";
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            List<Pictogram> options = new List<Pictogram> { _testContext.MockPictograms[PRIVATE_PICTOGRAM] };
            var res = choiceController.CreateChoice(new ChoiceDTO(new Choice(options, "TestChoice") { Id = CREATE_CHOICE_ID })).Result;

            Assert.IsType<Response<ChoiceDTO>>(res);
            Assert.True(res.Success);
            Assert.Equal(ErrorCode.NoError, res.ErrorCode);

            // Check data
            Assert.Equal(title, res.Data.Title);
            foreach (var option in options)
            {
                Assert.True(res.Data.Options.Any(o => o.Id == option.Id));
            }

        }

        [Fact]
        public void CreateChoice_OtherLoginCreatePrivate_Unauthorized()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            List<Pictogram> options = new List<Pictogram> { _testContext.MockPictograms[PRIVATE_PICTOGRAM] };
            var res = choiceController.CreateChoice(new ChoiceDTO(new Choice(options, "TestChoice") { Id = CREATE_CHOICE_ID })).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }
        #endregion

        #region UpdateChoice
        [Fact]
        public void UpdateChoice_NoLoginUpdatePublic_Ok()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            var title = "TestChoice";
            Choice c = new Choice(new List<Pictogram>(), title) { Id = _testContext.MockChoices[PUBLIC_CHOICE].Id };
            foreach (var option in _testContext.MockChoices[PUBLIC_CHOICE])
            {
                c.Add(option);
            }
            c.Clear();
            c.AddAll(_testContext.MockPictograms.Cast<Pictogram>().Where(p => p.AccessLevel == AccessLevel.PUBLIC).Take(TWO).ToList());

            var res = cc.UpdateChoice(c.Id, new ChoiceDTO(c)).Result;

            Assert.IsType<Response<ChoiceDTO>>(res);
            Assert.True(res.Success);
            Assert.Equal(ErrorCode.NoError, res.ErrorCode);

            // Check data
            Assert.Equal(title, res.Data.Title);
            foreach (var option in _testContext.MockChoices[PUBLIC_CHOICE])
            {
                Assert.True(res.Data.Options.Any(o => o.Id == option.Id));
            }
        }

        [Fact]
        public void UpdateChoice_LoginUpdatePublic_Ok()
        {
            var choiceController = initializeTest();
            var title = "TestChoice";
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            Choice c = new Choice(new List<Pictogram>(), title) { Id = _testContext.MockChoices[PUBLIC_CHOICE].Id };
            foreach (var option in _testContext.MockChoices[PUBLIC_CHOICE])
            {
                c.Add(option);
            }
            c.Clear();
            c.AddAll(_testContext.MockPictograms.Cast<Pictogram>().Where(p => p.AccessLevel == AccessLevel.PUBLIC).Take(TWO).ToList());

            var res = choiceController.UpdateChoice(c.Id, new ChoiceDTO(c)).Result;
            Assert.IsType<Response<ChoiceDTO>>(res);
            Assert.True(res.Success);
            Assert.Equal(ErrorCode.NoError, res.ErrorCode);

            // Check data
            Assert.Equal(title, res.Data.Title);
            foreach (var option in _testContext.MockChoices[PUBLIC_CHOICE])
            {
                Assert.True(res.Data.Options.Any(o => o.Id == option.Id));
            }
        }

        [Fact]
        public void UpdateChoice_NoLoginUpdatePrivate_Unauthorized()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            Choice c = new Choice(new List<Pictogram>(), "TestChoice") { Id = _testContext.MockChoices[PRIVATE_CHOICE].Id };
            foreach (var option in _testContext.MockChoices[PRIVATE_CHOICE])
            {
                c.Add(option);
            }
            c.Clear();
            c.AddAll(_testContext.MockPictograms.Cast<Pictogram>().Where(p => p.AccessLevel == AccessLevel.PUBLIC).Take(TWO).ToList());
            var res = cc.UpdateChoice(c.Id, new ChoiceDTO(c)).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }

        [Fact]
        public void UpdateChoice_LoginUpdatePrivate_Ok()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            var title = "TestChoice";

            Choice c = new Choice(new List<Pictogram>(), title) { Id = _testContext.MockChoices[PRIVATE_CHOICE].Id };
            foreach (var option in _testContext.MockChoices[PRIVATE_CHOICE])
            {
                c.Add(option);
            }
            c.Clear();
            c.AddAll(_testContext.MockPictograms.Cast<Pictogram>().Where(p => p.AccessLevel == AccessLevel.PUBLIC).Take(TWO).ToList());
            var res = choiceController.UpdateChoice(c.Id, new ChoiceDTO(c)).Result;

            Assert.IsType<Response<ChoiceDTO>>(res);
            Assert.True(res.Success);
            Assert.Equal(ErrorCode.NoError, res.ErrorCode);

            // Check data
            Assert.Equal(title, res.Data.Title);
            foreach (var option in _testContext.MockChoices[PRIVATE_CHOICE])
            {
                Assert.True(res.Data.Options.Any(o => o.Id == option.Id));
            }
        }


        [Fact]
        public void UpdateChoice_OtherLoginUpdatePrivate_Unauthorized()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            Choice c = new Choice(new List<Pictogram>(), "TestChoice") { Id = _testContext.MockChoices[PRIVATE_CHOICE].Id };
            foreach (var option in _testContext.MockChoices[PRIVATE_CHOICE])
            {
                c.Add(option);
            }
            c.Clear();
            c.AddAll(_testContext.MockPictograms.Cast<Pictogram>().Where(p => p.AccessLevel == AccessLevel.PUBLIC).Take(TWO).ToList());
            var res = choiceController.UpdateChoice(c.Id, new ChoiceDTO(c)).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }

        [Fact]
        public void UpdateChoice_NoLoginUpdateProtected_Unauthorized()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            Choice c = new Choice(new List<Pictogram>(), "TestChoice") { Id = _testContext.MockChoices[PROTECTED_CHOICE].Id };
            foreach (var option in _testContext.MockChoices[PROTECTED_CHOICE])
            {
                c.Add(option);
            }
            c.Clear();
            c.AddAll(_testContext.MockPictograms.Cast<Pictogram>().Where(p => p.AccessLevel == AccessLevel.PUBLIC).Take(TWO).ToList());
            var res = cc.UpdateChoice(c.Id, new ChoiceDTO(c)).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }

        [Fact]
        public void UpdateChoice_LoginUpdateProtected_Ok()
        {
            var choiceController = initializeTest();
            var title = "TestChoice";
                
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            Choice c = new Choice(new List<Pictogram>(), "TestChoice") { Id = _testContext.MockChoices[PROTECTED_CHOICE].Id };
            foreach (var option in _testContext.MockChoices[PROTECTED_CHOICE])
            {
                c.Add(option);
            }
            c.Clear();
            c.AddAll(_testContext.MockPictograms.Cast<Pictogram>().Where(p => p.AccessLevel == AccessLevel.PUBLIC).Take(TWO).ToList());
            var res = choiceController.UpdateChoice(c.Id, new ChoiceDTO(c)).Result;

            Assert.IsType<Response<ChoiceDTO>>(res);
            Assert.True(res.Success);
            Assert.Equal(ErrorCode.NoError, res.ErrorCode);

            // Check data
            Assert.Equal(title, res.Data.Title);
            foreach (var option in _testContext.MockChoices[PROTECTED_CHOICE])
            {
                Assert.True(res.Data.Options.Any(o => o.Id == option.Id));
            }
        }

        [Fact]
        public void UpdateChoice_OtherLoginUpdateProtected_Unauthorized()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            Choice c = new Choice(new List<Pictogram>(), "TestChoice") { Id = _testContext.MockChoices[PROTECTED_CHOICE].Id };
            foreach (var option in _testContext.MockChoices[PROTECTED_CHOICE])
            {
                c.Add(option);
            }
            c.Clear();
            c.AddAll(_testContext.MockPictograms.Cast<Pictogram>().Where(p => p.AccessLevel == AccessLevel.PUBLIC).Take(TWO).ToList());
            var res = choiceController.UpdateChoice(c.Id, new ChoiceDTO(c)).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }

        [Fact]
        public void UpdateChoice_NoLoginUpdateNonExisting_NotFound()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            Choice c = new Choice(new List<Pictogram>(), "TestChoice") { Id = NONEXISTING };
            c.AddAll(_testContext.MockPictograms.Cast<Pictogram>().Where(p => p.AccessLevel == AccessLevel.PUBLIC).Take(TWO).ToList());
            var res = cc.UpdateChoice(c.Id, new ChoiceDTO(c)).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotFound, res.ErrorCode);
        }

        [Fact]
        public void UpdateChoice_LoginUpdateNonExisting_NotFound()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            Choice c = new Choice(new List<Pictogram>(), "TestChoice") { Id = NONEXISTING };
            c.AddAll(_testContext.MockPictograms
                .Cast<Pictogram>()
                .Where(p => p.AccessLevel == AccessLevel.PUBLIC)
                .Take(TWO)
                .ToList());
            var res = choiceController.UpdateChoice(c.Id, new ChoiceDTO(c)).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotFound, res.ErrorCode);
        }
        #endregion

        #region DeleteChoice | Work as intented with the exception of the authorize attribute
        // ASP.NET inforces this attribute and some tests should therefore be changed to Unauthorized
        // if a Mock version of this attribute is made
        [Fact]
        public void DeleteChoice_NoLoginDeletePublic_Ok()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            var res = cc.DeleteChoice(_testContext.MockChoices[PUBLIC_CHOICE].Id).Result;

            Assert.IsType<Response>(res);
            Assert.True(res.Success);
        }

        [Fact]
        public void DeleteChoice_LoginDeletePublic_Ok()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            var res = choiceController.DeleteChoice(_testContext.MockChoices[PUBLIC_CHOICE].Id).Result;

            Assert.IsType<Response>(res);
            Assert.True(res.Success);
        }

        [Fact]
        public void DeleteChoice_NoLoginDeletePrivate_Unauthorized()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            var res = cc.DeleteChoice(_testContext.MockChoices[PRIVATE_CHOICE].Id).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }

        [Fact]
        public void DeleteChoice_LoginDeletePrivate_Ok()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            var res = choiceController.DeleteChoice(_testContext.MockChoices[PRIVATE_CHOICE].Id).Result;

            Assert.IsType<Response>(res);
            Assert.True(res.Success);
        }
        
        [Fact]
        public void DeleteChoice_OtherLoginDeletePrivate_Unauthorized()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            var res = choiceController.DeleteChoice(_testContext.MockChoices[PRIVATE_CHOICE].Id).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }
        
        [Fact]
        public void DeleteChoice_NoLoginDeleteProtected_Unauthorized()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            var res = cc.DeleteChoice(_testContext.MockChoices[PROTECTED_CHOICE].Id).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }
        
        [Fact]
        public void DeleteChoice_LoginDeleteProtected_Ok()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            var res = choiceController.DeleteChoice(_testContext.MockChoices[PROTECTED_CHOICE].Id).Result;

            Assert.IsType<Response>(res);
            Assert.True(res.Success);
        }

        [Fact]
        public void DeleteChoice_OtherLoginDeleteProtected_Unauthorized()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[GUARDIAN_DEP_TWO]);
            var res = choiceController.DeleteChoice(_testContext.MockChoices[PROTECTED_CHOICE].Id).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotAuthorized, res.ErrorCode);
        }

        [Fact]
        public void DeleteChoice_NoLoginDeleteNonExisting_NotFound()
        {
            var cc = initializeTest();
            _testContext.MockUserManager.MockLogout();
            var res = cc.DeleteChoice(NONEXISTING).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotFound, res.ErrorCode);
        }

        [Fact]
        public void DeleteChoice_LoginDeleteNonExisting_NotFound()
        {
            var choiceController = initializeTest();
            _testContext.MockUserManager.MockLoginAsUser(_testContext.MockUsers[ADMIN_DEP_ONE]);
            var res = choiceController.DeleteChoice(NONEXISTING).Result;

            Assert.IsType<ErrorResponse<ChoiceDTO>>(res);
            Assert.False(res.Success);
            Assert.Equal(ErrorCode.NotFound, res.ErrorCode);
        }
        #endregion
    }
}