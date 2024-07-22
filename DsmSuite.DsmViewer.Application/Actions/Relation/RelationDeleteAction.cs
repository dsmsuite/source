using DsmSuite.DsmViewer.Application.Actions.Base;
using DsmSuite.DsmViewer.Application.Actions.Management;
using DsmSuite.DsmViewer.Application.Interfaces;
using DsmSuite.DsmViewer.Model.Interfaces;
using System.Collections.Generic;

namespace DsmSuite.DsmViewer.Application.Actions.Relation
{
    public class RelationDeleteAction : IAction
    {
        private readonly IDsmModel _model;
        private readonly IActionContext _actionContext;
        private readonly IDsmRelation _relation;
        private readonly IDsmElement _consumer;
        private readonly IDsmElement _provider;

        public const ActionType RegisteredType = ActionType.RelationDelete;

        public RelationDeleteAction(IDsmModel model, IActionContext context, IReadOnlyDictionary<string, string> data)
        {
            _model = model;
            _actionContext = context;
            if (_model != null  &&  data != null)
            {
                ActionReadOnlyAttributes attributes = new ActionReadOnlyAttributes(_model, data);

                _relation = attributes.GetRelation(nameof(_relation));
                _consumer = attributes.GetRelationConsumer(nameof(_relation));
                _provider = attributes.GetRelationProvider(nameof(_relation));
            }
        }

        public RelationDeleteAction(IDsmModel model, IDsmRelation relation)
        {
            _model = model;
            _relation = relation;
            _consumer = model.GetElementById(_relation.Consumer.Id);
            _provider = model.GetElementById(_relation.Provider.Id);
        }

        public ActionType Type => RegisteredType;
        public string Title => "Delete relation";
        public string Description => $"consumer={_consumer.Fullname} provider={_provider.Fullname} type={_relation.Type}";

        public object Do()
        {
            _model.RemoveRelation(_relation.Id);
            return null;
        }

        public void Undo()
        {
            _model.UnremoveRelation(_relation.Id);
        }

        public bool IsValid()
        {
            return (_model != null) && 
                   (_relation != null) && 
                   (_consumer != null) && 
                   (_provider != null);
        }

        public IReadOnlyDictionary<string, string> Data
        {
            get
            {
                ActionAttributes attributes = new ActionAttributes();
                attributes.SetInt(nameof(_relation), _relation.Id);
                return attributes.Data;
            }
        }
    }
}
