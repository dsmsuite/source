using DsmSuite.DsmViewer.Application.Actions.Base;
using DsmSuite.DsmViewer.Application.Actions.Management;
using DsmSuite.DsmViewer.Application.Interfaces;
using DsmSuite.DsmViewer.Model.Interfaces;
using System.Collections.Generic;

namespace DsmSuite.DsmViewer.Application.Actions.Element
{
    public class ElementChangeParentAction : IAction
    {
        private readonly IDsmModel _model;
        private readonly IActionContext _actionContext;
        private readonly IDsmElement _element;
        private readonly IDsmElement _oldParent;
        private readonly int _oldIndex;
        private readonly string _oldName;
        private readonly IDsmElement _newParent;
        private readonly int _newIndex;
        private string _newName;

        public const ActionType RegisteredType = ActionType.ElementChangeParent;

        public ElementChangeParentAction(IDsmModel model, IActionContext context, IReadOnlyDictionary<string, string> data)
        {
            _model = model;
            _actionContext = context;
            if (_model != null  &&  data != null)
            {
                ActionReadOnlyAttributes attributes = new ActionReadOnlyAttributes(_model, data);

                _element = attributes.GetElement(nameof(_element));
                _oldParent = attributes.GetElement(nameof(_oldParent));
                _oldIndex = attributes.GetInt(nameof(_oldIndex));
                _oldName = attributes.GetString(nameof(_oldName));
                _newParent = attributes.GetElement(nameof(_newParent));
                _newIndex = attributes.GetInt(nameof(_newIndex));
                _newName = attributes.GetString(nameof(_newName));
            }
        }

        public ElementChangeParentAction(IDsmModel model, IDsmElement element, IDsmElement newParent, int index)
        {
            _model = model;
            _element = element;

            _oldParent = element.Parent;
            _oldIndex = _oldParent.IndexOfChild(element);
            _oldName = element.Name;

            _newParent = newParent;
            _newIndex = index;
            _newName = element.Name;
         }

        public ActionType Type => RegisteredType;
        public string Title => "Change element parent";
        public string Description => $"element={_element.Fullname} parent={_oldParent.Fullname}->{_newParent.Fullname}";

        public object Do()
        {
            // Rename to avoid duplicate names
            if (_newParent.ContainsChildWithName(_oldName))
            {
                _newName += " (duplicate)";
                _model.ChangeElementName(_element, _newName);
            }

            _model.ChangeElementParent(_element, _newParent, _newIndex);
            _model.AssignElementOrder();
            return null;
        }

        public void Undo()
        {
            _model.ChangeElementParent(_element, _oldParent, _oldIndex);
            _model.AssignElementOrder();

            // Restore original name
            if (_oldName != _newName)
            {
                _model.ChangeElementName(_element, _oldName);
            }
        }

        public bool IsValid()
        {
            return (_model != null) && 
                   (_element != null) && 
                   (_oldParent != null) && 
                   (_oldName != null) && 
                   (_newParent != null) && 
                   (_newName != null);
        }

        public IReadOnlyDictionary<string, string> Data
        {
            get
            {
                ActionAttributes attributes = new ActionAttributes();
                attributes.SetInt(nameof(_element), _element.Id);
                attributes.SetInt(nameof(_oldParent), _oldParent.Id);
                attributes.SetString(nameof(_oldName), _oldName);
                attributes.SetInt(nameof(_oldIndex), _oldIndex);
                attributes.SetInt(nameof(_newParent), _newParent.Id);
                attributes.SetString(nameof(_newName), _newName);
                attributes.SetInt(nameof(_newIndex), _newIndex);
                return attributes.Data;
            }
        }
    }
}
