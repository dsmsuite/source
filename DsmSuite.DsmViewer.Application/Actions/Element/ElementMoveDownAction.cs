using DsmSuite.DsmViewer.Application.Actions.Base;
using DsmSuite.DsmViewer.Application.Actions.Management;
using DsmSuite.DsmViewer.Application.Interfaces;
using DsmSuite.DsmViewer.Model.Interfaces;
using System.Collections.Generic;

namespace DsmSuite.DsmViewer.Application.Actions.Element
{
    public class ElementMoveDownAction : IAction
    {
        private readonly IDsmModel _model;
        private readonly IActionContext _actionContext;
        private readonly IDsmElement _element;

        public const ActionType RegisteredType = ActionType.ElementMoveDown;

        public ElementMoveDownAction(IDsmModel model, IActionContext context, IReadOnlyDictionary<string, string> data)
        {
            _model = model;
            _actionContext = context;
            if (_model != null  &&  data != null)
                _element = new ActionReadOnlyAttributes(_model, data).GetElement(nameof(_element));
        }

        public ElementMoveDownAction(IDsmModel model, IDsmElement element)
        {
            _model = model;
            _element = element;
        }

        public ActionType Type => RegisteredType;
        public string Title => "Move down element";
        public string Description => $"element={_element.Fullname}";

        public object Do()
        {
            IDsmElement nextElement = _model.NextSibling(_element);
            if (nextElement != null)
            {
                _model.Swap(_element, nextElement);
                _model.AssignElementOrder();
            }

            return null;
        }

        public void Undo()
        {
            IDsmElement previousElement = _model.PreviousSibling(_element);
            if (previousElement != null)
            {
                _model.Swap(previousElement, _element);
                _model.AssignElementOrder();
            }
        }

        public bool IsValid()
        {
            return (_model != null) && 
                   (_element != null);
        }

        public IReadOnlyDictionary<string, string> Data
        {
            get
            {
                ActionAttributes attributes = new ActionAttributes();
                attributes.SetInt(nameof(_element), _element.Id);
                return attributes.Data;
            }
        }
    }
}
