using UnityEngine;

namespace StarboundSprint.World
{
    public class LevelEditorStub : MonoBehaviour
    {
        [SerializeField] private bool editorEnabled;

        public bool EditorEnabled => editorEnabled;

        public void ToggleEditor(bool enabled)
        {
            editorEnabled = enabled;
        }
    }
}
