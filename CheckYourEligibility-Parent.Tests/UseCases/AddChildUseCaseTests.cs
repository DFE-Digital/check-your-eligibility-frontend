using System.Collections.Generic;
using System.Linq;
using System;

namespace CheckYourEligibility_Parent.Tests.UseCases
{
    [TestFixture]
    public class AddChildUseCaseTests
    {
        private AddChildUseCase _sut;
        private Mock<ITempDataDictionary> _tempDataMock;
        private Children _children;

        [SetUp]
        public void Setup()
        {
            _tempDataMock = new Mock<ITempDataDictionary>();
            _sut = new AddChildUseCase();
            _children = new Children { ChildList = new List<Child>() };
        }

        [Test]
        public void ExecuteAsync_WhenChildrenIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => _sut.ExecuteAsync(null, _tempDataMock.Object))
                .Should().Throw<ArgumentNullException>()
                .WithParameterName("request");
        }

        [Test]
        public void ExecuteAsync_WhenTempDataIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            FluentActions.Invoking(() => _sut.ExecuteAsync(_children, null))
                .Should().Throw<ArgumentNullException>()
                .WithParameterName("tempData");
        }

        [Test]
        public void ExecuteAsync_WhenChildrenExceedsMaxLimit_ReturnsFalse()
        {
            // Arrange
            _children.ChildList = Enumerable.Range(0, 99).Select(_ => new Child()).ToList();

            // Act
            var result = _sut.ExecuteAsync(_children, _tempDataMock.Object);

            // Assert
            result.Should().BeFalse();
            _tempDataMock.VerifySet(x => x["IsChildAddOrRemove"] = It.IsAny<bool>(), Times.Never);
            _tempDataMock.VerifySet(x => x["ChildList"] = It.IsAny<string>(), Times.Never);
        }

        [Test]
        public void ExecuteAsync_WhenValidRequest_AddsChildAndUpdatesTempData()
        {
            // Arrange
            _children.ChildList = new List<Child> { new Child() };
            string capturedJson = null;
            _tempDataMock.SetupSet(x => x["ChildList"] = It.IsAny<string>())
                .Callback<string>((value) => capturedJson = value);

            // Act
            var result = _sut.ExecuteAsync(_children, _tempDataMock.Object);

            // Assert
            result.Should().BeTrue();
            _tempDataMock.VerifySet(x => x["IsChildAddOrRemove"] = true, Times.Once);
            _tempDataMock.VerifySet(x => x["ChildList"] = It.IsAny<string>(), Times.Once);

            var deserializedChildren = JsonConvert.DeserializeObject<List<Child>>(capturedJson);
            deserializedChildren.Should().HaveCount(2);
            deserializedChildren.Last().Should().BeEquivalentTo(new Child());
        }

        [Test]
        public void ExecuteAsync_WhenAddingChild_SetsCorrectTempDataFlags()
        {
            // Act
            var result = _sut.ExecuteAsync(_children, _tempDataMock.Object);

            // Assert
            result.Should().BeTrue();
            _tempDataMock.VerifySet(x => x["IsChildAddOrRemove"] = true, Times.Once);
        }
    }
}