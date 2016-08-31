using UnityEditor;

public class ImportWaiter
{
    static void CloseAfterImporting()
    {
        EditorApplication.Exit(0);
    }
}
