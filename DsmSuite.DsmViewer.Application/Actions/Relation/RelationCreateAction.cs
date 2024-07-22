using DsmSuite.DsmViewer.Application.Actions.Base;
using DsmSuite.DsmViewer.Application.Actions.Management;
using DsmSuite.DsmViewer.Application.Interfaces;
using DsmSuite.DsmViewer.Model.Interfaces;
using System.Collections.Generic;

namespace DsmSuite.DsmViewer.Application.Actions.Relation
{
    public class RelationCreateAction : IAction
    {
        private readonly IDsmModel _model;
        private readonly IActionContext _actionContext;
        private IDsmRelation _relation;
        private readonly IDsmElement _consumer;
        private readonly IDsmElement _provider;
        private readonly string _type;
        private readonly int _weight;

        public const ActionType RegisteredType = ActionType.RelationCreate;

        public RelationCreateAction(IDsmModel model, IActionContext context, IReadOnlyDictionary<string, string> data)
        {
            _model = model;
            _actionContext = context;
            if (_model != null  &&  data != null)
            {
                ActionReadOnlyAttributes attributes = new ActionReadOnlyAttributes(_model, data);

                _relation = attributes.GetRelation(nameof(_relation));
                _consumer = attributes.GetRelationConsumer(nameof(_relation));
                _provider = attributes.GetRelationProvider(nameof(_relation));
                _type = attributes.GetString(nameof(_type));
                _weight = attributes.GetInt(nameof(_weight));
            }
        }

        public RelationCreateAction(IDsmModel model, int consumerId, int providerId, string type, int weight)
        {
            _model = model;
            _consumer = model.GetElementById(consumerId);
            _provider = model.GetElementById(providerId);
            _type = type;
            _weight = weight;
        }

        public ActionType Type => RegisteredType;
        public string Title => "Create relation";
        public string Description => $"consumer={_consumer.Fullname} provider={_provider.Fullname} type={_type} weight={_weight}";

        public object Do()
        {
            return _model.AddRelation(_consumer, _provider, _type, _weight, null);
        }

        public void Undo()
        {
            _model.RemoveRelation(_relation.Id);
        }

        public bool IsValid()
        {
            return (_model != null) && 
                   (_relation != null) &&
                   (_provider != null) &&
                   (_provider != null) &&
                   (_type != null);
        }

        public IReadOnlyDictionary<string, string> Data
        {
            get
            {
                ActionAttributes attributes = new ActionAttributes();
                attributes.SetInt(nameof(_relation), _relation.Id);
                attributes.SetInt(nameof(_consumer), _consumer.Id);
                attributes.SetInt(nameof(_provider), _provider.Id);
                attributes.SetString(nameof(_type), _type);
                attributes.SetInt(nameof(_weight), _weight);
                return attributes.Data;
            }
        }
    }
}
