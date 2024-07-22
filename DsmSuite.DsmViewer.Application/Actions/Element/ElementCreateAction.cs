using DsmSuite.DsmViewer.Application.Actions.Base;
using DsmSuite.DsmViewer.Application.Actions.Management;
using DsmSuite.DsmViewer.Application.Interfaces;
using DsmSuite.DsmViewer.Model.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace DsmSuite.DsmViewer.Application.Actions.Element
{
    public class ElementCreateAction : IAction
    {
        private readonly IDsmModel _model;
        private readonly IActionContext _actionContext;
        private IDsmElement _element;
        private readonly string _name;
        private readonly string _type;
        private readonly IDsmElement _parent;
        private readonly int _index;

        public const ActionType RegisteredType = ActionType.ElementCreate;

        public ElementCreateAction(IDsmModel model, IActionContext context, IReadOnlyDictionary<string, string> data)
        {
            _model = model;
            _actionContext = context;
            if (_model != null  &&  data != null)
            {
                ActionReadOnlyAttributes attributes = new ActionReadOnlyAttributes(_model, data);

                _element = attributes.GetElement(nameof(_element));
                _name = attributes.GetString(nameof(_name));
                _type = attributes.GetString(nameof(_type));

                int? parentId = attributes.GetNullableInt(nameof(_parent));
                if (parentId.HasValue)
                {
                    _parent = _model.GetElementById(parentId.Value);
                }
                _index = attributes.GetInt(nameof(_index));
            }
        }

        public ElementCreateAction(IDsmModel model, string name, string type, IDsmElement parent, int index)
        {
            _model = model;
            _name = name;
            _type = type;
            _parent = parent;
            _index = index;
        }

        public ActionType Type => RegisteredType;
        public string Title => "Create element";
        public string Description => $"name={_name} type={_type} parent={_parent.Fullname}";

        public object Do()
        {
            _element = _model.AddElement(_name, _type, _parent.Id, _index, null);
            Debug.Assert(_element != null);

            _model.AssignElementOrder();

            return _element;
        }

        public void Undo()
        {
            _model.RemoveElement(_element.Id);
            _model.AssignElementOrder();
        }

        public bool IsValid()
        {
            return (_model != null) && 
                   (_element != null) && 
                   (_type != null) && 
                   (_parent != null);
        }

        public IReadOnlyDictionary<string, string> Data
        {
            get
            {
                ActionAttributes attributes = new ActionAttributes();
                attributes.SetInt(nameof(_element), _element.Id);
                attributes.SetString(nameof(_name), _name);
                attributes.SetString(nameof(_type), _type);
                attributes.SetNullableInt(nameof(_parent), _parent.Id);
                attributes.SetInt(nameof(_index), _index);
                return attributes.Data;
            }
        }
    }
}
