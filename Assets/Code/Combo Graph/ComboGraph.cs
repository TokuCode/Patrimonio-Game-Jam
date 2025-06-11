using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Movement3D.ComboGraph
{
    public class ComboGraph : EditorWindow
    {
        private ComboGraphView _graphView;
        private string _fileName = "Basic";
        
        [MenuItem("Graph/Combo Graph")]
        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<ComboGraph>();
            window.titleContent = new GUIContent("Combo Graph");
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        private void ConstructGraphView()
        {
            _graphView = new ComboGraphView(this)
            {
                name = "Combo Graph"
            };
            
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var fileNameTextField = new TextField("File Name");
            fileNameTextField.SetValueWithoutNotify(_fileName);
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback( evt => _fileName = evt.newValue );
            toolbar.Add(fileNameTextField);
            
            toolbar.Add(new Button( () => RequestDataOperation(true)) {text = "Save Data"});
            toolbar.Add(new Button( () => RequestDataOperation(false)) {text = "Load Data"});
            
            var nodeCreateButton = new Button(() => { _graphView.CreateNode("Combo Node", Vector2.zero); });
            nodeCreateButton.text = "Create Node";
            toolbar.Add(nodeCreateButton);
            
            rootVisualElement.Add(toolbar);
        }

        private void RequestDataOperation(bool save)
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name", "OK");
                return;
            }

            var saveUtility = GraphSaveUtility.GetInstance(_graphView);
            
            if(save) saveUtility.SaveGraph(_fileName);
            else saveUtility.LoadGraph(_fileName);
        }
    }
}
