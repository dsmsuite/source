using Microsoft.VisualStudio.TestTools.UnitTesting;
using DsmSuite.DsmViewer.Model.Interfaces;
using Moq;
using System.Collections.Generic;
using DsmSuite.DsmViewer.Application.Actions.Element;
using System;

namespace DsmSuite.DsmViewer.Application.Test.Actions.Element
{
    [TestClass]
    public class ElementChangeParentActionTest
    {
        private Mock<IDsmModel> _model;
        private Mock<IDsmElement> _element;
        private Mock<IDsmElement> _oldParent;
        private Mock<IDsmElement> _newParent;

        private Dictionary<string, string> _data;

        private const int ElementId = 1;
        private const int OldParentId = 2;
        private const int NewParentId = 3;

        private const int OldIndex = 4;
        private const int NewIndex = 5;

        private const string OldName = "OldName";
        private const string NewName = "OldName (duplicate)";

        [TestInitialize()]
        public void Setup()
        {
            _model = new Mock<IDsmModel>();
            _element = new Mock<IDsmElement>();
            _oldParent = new Mock<IDsmElement>();
            _newParent = new Mock<IDsmElement>();

            _element.Setup(x => x.Id).Returns(ElementId);
            _element.Setup(x => x.Parent).Returns(_oldParent.Object);
            _element.Setup(x => x.Name).Returns(OldName);
            _oldParent.Setup(x => x.IndexOfChild(_element.Object)).Returns(OldIndex);
            _oldParent.Setup(x => x.Id).Returns(OldParentId);
            _newParent.Setup(x => x.Id).Returns(NewParentId);

            _model.Setup(x => x.GetElementById(ElementId)).Returns(_element.Object);
            _model.Setup(x => x.GetElementById(OldParentId)).Returns(_oldParent.Object);
            _model.Setup(x => x.GetElementById(NewParentId)).Returns(_newParent.Object);

            _data = new Dictionary<string, string>
            {
                ["element"] = ElementId.ToString(),
                ["oldParent"] = OldParentId.ToString(),
                ["oldIndex"] = OldIndex.ToString(),
                ["oldName"] = OldName,
                ["newParent"] = NewParentId.ToString(),
                ["newIndex"] = NewIndex.ToString(),
                ["newName"] = NewName,
            };
        }

        [TestMethod]
        public void WhenDoActionThenElementParentIsChangedDataModelNoNameChange()
        {
            _newParent.Setup(x => x.ContainsChildWithName(OldName)).Returns(false);

            ElementChangeParentAction action =
                new ElementChangeParentAction(_model.Object, _element.Object, _newParent.Object, NewIndex);
            action.Do();
            Assert.IsTrue(action.IsValid());

            _model.Verify(x => x.ChangeElementParent(_element.Object, _newParent.Object, NewIndex), Times.Once());
            _model.Verify(x => x.ChangeElementName(It.IsAny<IDsmElement>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void WhenDoActionThenElementParentIsChangedDataModelNameChange()
        {
            _newParent.Setup(x => x.ContainsChildWithName(OldName)).Returns(true);

            ElementChangeParentAction action =
                new ElementChangeParentAction(_model.Object, _element.Object, _newParent.Object, NewIndex);
            action.Do();
            Assert.IsTrue(action.IsValid());

            _model.Verify(x => x.ChangeElementParent(_element.Object, _newParent.Object, NewIndex), Times.Once());
            _model.Verify(x => x.ChangeElementName(_element.Object, NewName), Times.Once());
        }

        [TestMethod]
        public void WhenUndoActionThenElementParentIsRevertedDataModelNoNameChange()
        {
            _newParent.Setup(x => x.ContainsChildWithName(OldName)).Returns(false);

            ElementChangeParentAction action =
                new ElementChangeParentAction(_model.Object, _element.Object, _newParent.Object, NewIndex);
            action.Undo();
            Assert.IsTrue(action.IsValid());

            _model.Verify(x => x.ChangeElementParent(_element.Object, _oldParent.Object, OldIndex), Times.Once());
            _model.Verify(x => x.ChangeElementName(It.IsAny<IDsmElement>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void WhenUndoActionThenElementParentIsRevertedDataModelNameChange()
        {
            string elementName = null;

            _newParent.Setup(x => x.ContainsChildWithName(OldName)).Returns(true);
            _model.Setup(x => x.ChangeElementName(It.IsAny<IDsmElement>(), It.IsAny<string>()))
                    .Callback((IDsmElement e, string name) => { elementName = name; });

            ElementChangeParentAction action =
                new ElementChangeParentAction(_model.Object, _element.Object, _newParent.Object, NewIndex);
            // Do saves state in Data so we cannot test Undo() without calling Do
            action.Do();
            action.Undo();
            Assert.IsTrue(action.IsValid());

            _model.Verify(x => x.ChangeElementParent(_element.Object, _oldParent.Object, OldIndex), Times.Once());
            _model.Verify(x => x.ChangeElementName(_element.Object, OldName), Times.Once());
            Assert.AreEqual(elementName, OldName);
        }

        [TestMethod]
        public void GivenLoadedActionWhenGettingDataThenActionAttributesMatch()
        {
            object[] args = {_model.Object, null, _data};
            ElementChangeParentAction action =
                    Activator.CreateInstance(typeof(ElementChangeParentAction), args) as ElementChangeParentAction;

            Assert.AreEqual(7, action.Data.Count);
            Assert.AreEqual(ElementId.ToString(), _data["element"]);
            Assert.AreEqual(OldParentId.ToString(), _data["oldParent"]);
            Assert.AreEqual(OldIndex.ToString(), _data["oldIndex"]);
            Assert.AreEqual(OldName, _data["oldName"]);
            Assert.AreEqual(NewParentId.ToString(), _data["newParent"]);
            Assert.AreEqual(NewIndex.ToString(), _data["newIndex"]);
            Assert.AreEqual(NewName, _data["newName"]);
        }
    }
}
