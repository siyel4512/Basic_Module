using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Action/NagativeCount", fileName = "Nagative Count")]
public class NagativeCount : TaskAction
{
    public override int Run(Task task, int currentSuccess, int successCount)
    {
        return successCount < 0 ? currentSuccess - successCount : currentSuccess;
    }
}
