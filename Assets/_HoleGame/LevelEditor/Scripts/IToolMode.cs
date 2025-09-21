using UnityEngine;
using System.Collections.Generic;

public interface IToolMode
{
    void OnInit();
    void OnToolEnable();
    void OnToolDisable();
    void OnUpdate();

    void OnEscapeKey();
    string ToolName { get; }
    string ExternalUsage { get; }
} 