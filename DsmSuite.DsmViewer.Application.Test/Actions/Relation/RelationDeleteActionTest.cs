using Microsoft.VisualStudio.TestTools.UnitTesting;
using DsmSuite.DsmViewer.Model.Interfaces;
using Moq;
using DsmSuite.DsmViewer.Application.Actions.Relation;
using System.Collections.Generic;
using System;

namespace DsmSuite.DsmViewer.Application.Test.Actions.Relation
{
    [TestClass]
    public class RelationDeleteActionTest
    {
        private Mock<IDsmModel> _model;
        private Mock<IDsmRelation> _relation;
        private Mock<IDsmElement> _consumer;
        private Mock<IDsmElement> _provider;

        private Dictionary<string, string> _data;

        private const int RelationId = 1;
        private const int ConsumerId = 2;
        private const int ProviderId = 3;

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
            _model.Setup(x => x.GetElementById(ConsumerId)).Returns(_consumer.Object);
            _model.Setup(x => x.GetElementById(ProviderId)).Returns(_provider.Object);

            _data = new Dictionary<string, string>
            {
                ["relation"] = RelationId.ToString()
            };
        }

        [TestMethod]
        public void WhenDoActionThenRelationIsRemovedFromDataModel()
        {
            RelationDeleteAction action = new RelationDeleteAction(_model.Object, _relation.Object);
            action.Do();

            _model.Verify(x => x.RemoveRelation(RelationId), Times.Once());
        }

        [TestMethod]
        public void WhenUndoActionThenRelationIsRestoredInDataModel()
        {
            RelationDeleteAction action = new RelationDeleteAction(_model.Object, _relation.Object);
            action.Undo();

            _model.Verify(x => x.UnremoveRelation(RelationId), Times.Once());
        }

        [TestMethod]
        public void GivenLoadedActionWhenGettingDataThenActionAttributesMatch()
        {
            _model.Setup(x => x.GetDeletedRelationById(RelationId)).Returns(_relation.Object);

            object[] args = { _model.Object, null, _data };
            RelationDeleteAction action = Activator.CreateInstance(typeof(RelationDeleteAction), args) as RelationDeleteAction;

            Assert.AreEqual(1, action.Data.Count);
            Assert.AreEqual(RelationId.ToString(), _data["relation"]);
        }
    }
}
