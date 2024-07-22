using DsmSuite.DsmViewer.Application.Actions.Base;
using DsmSuite.DsmViewer.Application.Actions.Management;
using DsmSuite.DsmViewer.Application.Interfaces;
using DsmSuite.DsmViewer.Model.Interfaces;
using System.Collections.Generic;

namespace DsmSuite.DsmViewer.Application.Actions.Relation
{
    public class RelationChangeWeightAction : IAction
    {
        private readonly IDsmModel _model;
        private readonly IActionContext _actionContext;
        private readonly IDsmRelation _relation;
        private readonly IDsmElement _consumer;
        private readonly IDsmElement _provider;
        private readonly int _old;
        private readonly int _new;

        public const ActionType RegisteredType = ActionType.RelationChangeWeight;

        public RelationChangeWeightAction(IDsmModel model, IActionContext context, IReadOnlyDictionary<string, string> data)
        {
            _model = model;
            _actionContext = context;
            if (_model != null  &&  data != null)
            {
                ActionReadOnlyAttributes attributes = new ActionReadOnlyAttributes(_model, data);

                _relation = attributes.GetRelation(nameof(_relation));
                _consumer = attributes.GetRelationConsumer(nameof(_relation));
                _provider = attributes.GetRelationProvider(nameof(_relation));
                _old = attributes.GetInt(nameof(_old));
                _new = attributes.GetInt(nameof(_new));
            }
        }

        public RelationChangeWeightAction(IDsmModel model, IDsmRelation relation, int weight)
        {
            _model = model;
            _relation = relation;
            _consumer = model.GetElementById(_relation.Consumer.Id);
            _provider = model.GetElementById(_relation.Provider.Id);
            _old = relation.Weight;
            _new = weight;
        }

        public ActionType Type => RegisteredType;
        public string Title => "Change relation weight";
        public string Description => $"consumer={_consumer.Fullname} provider={_provider.Fullname} type={_relation.Type} weight={_old}->{_new}";

        public object Do()
        {
            _model.ChangeRelationWeight(_relation, _new);
            return null;
        }

        public void Undo()
        {
            _model.ChangeRelationWeight(_relation, _old);
        }

        public bool IsValid()
        {
            return (_model != null) && 
                   (_relation != null);
        }

        public IReadOnlyDictionary<string, string> Data
        {
            get
            {
                ActionAttributes attributes = new ActionAttributes();
                attributes.SetInt(nameof(_relation), _relation.Id);
                attributes.SetInt(nameof(_old), _old);
                attributes.SetInt(nameof(_new), _new);
                return attributes.Data;
            }
        }
    }
}
