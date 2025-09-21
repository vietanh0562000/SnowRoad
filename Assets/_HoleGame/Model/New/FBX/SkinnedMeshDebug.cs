using UnityEngine;

namespace PuzzleGames
{
    using UnityEngine;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class SkinnedMeshDebug : MonoBehaviour
{
    public SkinnedMeshRenderer targetRenderer;   // Rig đang dùng trong game
    public SkinnedMeshRenderer sourceRenderer;   // Mesh mới muốn kiểm tra

    [Button]
    [ContextMenu("Check Mesh Compatibility")]
    void CheckMeshCompatibility()
    {
        if (targetRenderer == null || sourceRenderer == null)
        {
            Debug.LogError("Chưa gán targetRenderer hoặc sourceRenderer");
            return;
        }

        Transform[] targetBones = targetRenderer.bones;
        Transform[] sourceBones = sourceRenderer.bones;

        List<string> missingBones = new List<string>();
        List<string> bindPoseMismatch = new List<string>();
        List<string> extraBones = new List<string>();

        Matrix4x4[] targetBindPoses = targetRenderer.sharedMesh.bindposes;
        Matrix4x4[] sourceBindPoses = sourceRenderer.sharedMesh.bindposes;

        // --- Check bone thiếu & bind pose ---
        for (int i = 0; i < sourceBones.Length; i++)
        {
            string boneName = sourceBones[i] != null ? sourceBones[i].name : $"<null_{i}>";
            Transform matchBone = FindBoneByName(targetBones, boneName);

            if (matchBone == null)
            {
                missingBones.Add(boneName);
            }
            else
            {
                // So sánh bind pose
                int targetIndex = FindBoneIndexByName(targetBones, boneName);
                if (targetIndex >= 0 && targetIndex < targetBindPoses.Length && i < sourceBindPoses.Length)
                {
                    if (!MatrixEqual(targetBindPoses[targetIndex], sourceBindPoses[i]))
                        bindPoseMismatch.Add(boneName);
                }
            }
        }

        // --- Check bone thừa ---
        foreach (var tb in targetBones)
        {
            if (tb == null) continue;
            bool used = false;
            foreach (var sb in sourceBones)
            {
                if (sb != null && sb.name == tb.name)
                {
                    used = true;
                    break;
                }
            }
            if (!used)
                extraBones.Add(tb.name);
        }

        // --- Kết quả ---
        Debug.Log($"=== Kết quả kiểm tra mesh '{sourceRenderer.sharedMesh.name}' ===");

        if (missingBones.Count > 0)
            Debug.LogWarning($"❌ Bone thiếu (mesh cần nhưng rig không có): {string.Join(", ", missingBones)}");
        else
            Debug.Log("✅ Không thiếu bone nào");

        if (bindPoseMismatch.Count > 0)
            Debug.LogWarning($"⚠️ Bind pose khác: {string.Join(", ", bindPoseMismatch)}");
        else
            Debug.Log("✅ Bind pose khớp");

        if (extraBones.Count > 0)
            Debug.Log($"ℹ️ Bone thừa (rig có nhưng mesh không dùng): {string.Join(", ", extraBones)}");
        else
            Debug.Log("✅ Không có bone thừa");

        if (missingBones.Count == 0 && bindPoseMismatch.Count == 0)
            Debug.Log("🎯 Mesh tương thích hoàn toàn!");
    }

    Transform FindBoneByName(Transform[] bones, string name)
    {
        foreach (var b in bones)
            if (b != null && b.name == name)
                return b;
        return null;
    }

    int FindBoneIndexByName(Transform[] bones, string name)
    {
        for (int i = 0; i < bones.Length; i++)
            if (bones[i] != null && bones[i].name == name)
                return i;
        return -1;
    }

    bool MatrixEqual(Matrix4x4 a, Matrix4x4 b, float tolerance = 0.0001f)
    {
        for (int i = 0; i < 16; i++)
            if (Mathf.Abs(a[i] - b[i]) > tolerance)
                return false;
        return true;
    }
}

}