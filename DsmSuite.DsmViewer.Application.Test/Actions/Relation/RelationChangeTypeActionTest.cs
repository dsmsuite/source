using Microsoft.VisualStudio.TestTools.UnitTesting;
using DsmSuite.DsmViewer.Model.Interfaces;
using Moq;
using DsmSuite.DsmViewer.Application.Actions.Relation;
using System.Collections.Generic;
using System;

namespace DsmSuite.DsmViewer.Application.Test.Actions.Relation
{
    [TestClass]
    public class RelationChangeTypeActionTest
    {
        private Mock<IDsmModel> _model;
        private Mock<IDsmRelation> _relation;
        private Mock<IDsmElement> _consumer;
        private Mock<IDsmElement> _provider;

        private Dictionary<string, string> _data;

        private const int RelationId = 1;
        private const int ConsumerId = 2;
        private const int ProviderId = 3;
        private const string OldType = "oldtype";
        private const string NewType = "newtype";

        [TestInitialize()]
        public void Setup()
        {
            _model = new Mock<IDsmModel>();
            _relation = new Mock<IDsmRelation>();
            _consumer = new Mock<IDsmElement>();
            _provider = new Mock<IDsmElement>();

            _relation.Setup(x => x.Id).Returns(RelationId);
            _relation.Setup(x => x.Consumer.Id).Returns(ConsumerId);
            _relation.Setup(x => x.Provider.Id).Returns(ProviderId);
            _relation.Setup(x => x.Type).Returns(OldType);
            _model.Setup(x => x.GetElementById(ConsumerId)).Returns(_consumer.Object);
            _model.Setup(x => x.GetElementById(ProviderId)).Returns(_provider.Object);

            _data = new Dictionary<string, string>
            {
                ["relation"] = RelationId.ToString(),
                ["old"] = OldType,
                ["new"] = NewType
            };
        }

        [TestMethod]
        public void WhenDoActionThenRelationTypeIsChangedDataModel()
        {
            RelationChangeTypeAction action = new RelationChangeTypeAction(_model.Object, _relation.Object, NewType);
            action.Do();

            _model.Verify(x => x.ChangeRelationType(_relation.Object, NewType), Times.Once());
        }

        [TestMethod]
        public void WhenUndoActionThenRelationTypeIsRevertedDataModel()
        {
            RelationChangeTypeAction action = new RelationChangeTypeAction(_model.Object, _relation.Object, NewType);
            action.Undo();

            _model.Verify(x => x.ChangeRelationType(_relation.Object, OldType), Times.Once());
        }

        [TestMethod]
        public void GivenLoadedActionWhenGettingDataThenActionAttributesMatch()
        {
            _model.Setup(x => x.GetRelationById(RelationId)).Returns(_relation.Object);

            object[] args = { _model.Object, null, _data };
            RelationChangeTypeAction action =
                    Activator.CreateInstance(typeof(RelationChangeTypeAction), args) as RelationChangeTypeAction;

            Assert.AreEqual(3, action.Data.Count);
            Assert.AreEqual(RelationId.ToString(), _data["relation"]);
            Assert.AreEqual(OldType, _data["old"]);
            Assert.AreEqual(NewType, _data["new"]);
        }
    }
}
