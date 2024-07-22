using Microsoft.VisualStudio.TestTools.UnitTesting;
using DsmSuite.DsmViewer.Model.Interfaces;
using Moq;
using DsmSuite.DsmViewer.Application.Actions.Relation;
using System.Collections.Generic;
using System;

namespace DsmSuite.DsmViewer.Application.Test.Actions.Relation
{
    [TestClass]
    public class RelationChangeWeightActionTest
    {
        private Mock<IDsmModel> _model;
        private Mock<IDsmRelation> _relation;
        private Mock<IDsmElement> _consumer;
        private Mock<IDsmElement> _provider;

        private Dictionary<string, string> _data;

        private const int RelationId = 1;
        private const int ConsumerId = 2;
        private const int ProviderId = 3;
        private const int OldWeight = 123;
        private const int NewWeight = 456;

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
            _relation.Setup(x => x.Weight).Returns(OldWeight);
            _model.Setup(x => x.GetElementById(ConsumerId)).Returns(_consumer.Object);
            _model.Setup(x => x.GetElementById(ProviderId)).Returns(_provider.Object);

            _data = new Dictionary<string, string>
            {
                ["relation"] = RelationId.ToString(),
                ["old"] = OldWeight.ToString(),
                ["new"] = NewWeight.ToString()
            };
        }

        [TestMethod]
        public void WhenDoActionThenRelationWeightIsChangedDataModel()
        {
            RelationChangeWeightAction action = new RelationChangeWeightAction(_model.Object, _relation.Object, NewWeight);
            action.Do();

            _model.Verify(x => x.ChangeRelationWeight(_relation.Object, NewWeight), Times.Once());
        }

        [TestMethod]
        public void WhenUndoActionThenRelationWeightIsRevertedDataModel()
        {
            RelationChangeWeightAction action = new RelationChangeWeightAction(_model.Object, _relation.Object, NewWeight);
            action.Undo();

            _model.Verify(x => x.ChangeRelationWeight(_relation.Object, OldWeight), Times.Once());
        }

        [TestMethod]
        public void GivenLoadedActionWhenGettingDataThenActionAttributesMatch()
        {
            _model.Setup(x => x.GetRelationById(RelationId)).Returns(_relation.Object);

            object[] args = { _model.Object, null, _data };
            RelationChangeWeightAction action =
                    Activator.CreateInstance(typeof(RelationChangeWeightAction), args) as RelationChangeWeightAction;

            Assert.AreEqual(3, action.Data.Count);
            Assert.AreEqual(RelationId.ToString(), _data["relation"]);
            Assert.AreEqual(OldWeight.ToString(), _data["old"]);
            Assert.AreEqual(NewWeight.ToString(), _data["new"]);
        }
    }
}
