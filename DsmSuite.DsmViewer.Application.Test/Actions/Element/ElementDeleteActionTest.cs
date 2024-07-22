using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using DsmSuite.DsmViewer.Model.Interfaces;
using DsmSuite.DsmViewer.Application.Actions.Element;
using System.Collections.Generic;
using System;

namespace DsmSuite.DsmViewer.Application.Test.Actions.Element
{
    [TestClass]
    public class ElementDeleteActionTest
    {
        private Mock<IDsmModel> _model;
        private Mock<IDsmElement> _element;

        private Dictionary<string, string> _data;

        private const int ElementId = 1;

        [TestInitialize()]
        public void Setup()
        {
            _model = new Mock<IDsmModel>();
            _element = new Mock<IDsmElement>();
            _element.Setup(x => x.Id).Returns(ElementId);
            _model.Setup(x => x.GetDeletedElementById(ElementId)).Returns(_element.Object);

            _data = new Dictionary<string, string>
            {
                ["element"] = ElementId.ToString()
            };
        }

        [TestMethod]
        public void WhenDoActionThenElementIsRemovedFromDataModel()
        {
            ElementDeleteAction action = new ElementDeleteAction(_model.Object, _element.Object);
            action.Do();
            Assert.IsTrue(action.IsValid());

            _model.Verify(x => x.RemoveElement(ElementId), Times.Once());
        }

        [TestMethod]
        public void WhenUndoActionThenElementIsRestoredInDataModel()
        {
            ElementDeleteAction action = new ElementDeleteAction(_model.Object, _element.Object);
            action.Undo();
            Assert.IsTrue(action.IsValid());

            _model.Verify(x => x.UnremoveElement(ElementId), Times.Once());
        }

        [TestMethod]
        public void GivenLoadedActionWhenGettingDataThenActionAttributesMatch()
        {
            object[] args = { _model.Object, null, _data };
            ElementDeleteAction action =
                    Activator.CreateInstance(typeof(ElementDeleteAction), args) as ElementDeleteAction;

            Assert.AreEqual(1, action.Data.Count);
            Assert.AreEqual(ElementId.ToString(), _data["element"]);
        }
    }
}
