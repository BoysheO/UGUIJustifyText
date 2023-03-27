using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable IdentifierTypo
namespace BoysheO.UnityComponent
{
    /// <summary>
    /// UGUIText两端对齐
    /// </summary>
    [RequireComponent(typeof(Text))]
    [ExecuteAlways]
    public sealed class UGUIJustifyText : MonoBehaviour, IMeshModifier
    {
        private readonly Dictionary<int, float> cache_lineIdx_minX = new Dictionary<int, float>();
        private readonly Dictionary<int, float> cache_lineIdx_maxX = new Dictionary<int, float>();
        private readonly Dictionary<int, Vector2> LineInterval = new Dictionary<int, Vector2>();

        private void OnEnable()
        {
            Text.SetVerticesDirty();
        }

        private void OnDisable()
        {
            Text.SetVerticesDirty();
        }

        /// <summary>
        /// 计算结果保存在<see cref="LineInterval"/>
        /// </summary>
        private void UpdateLineInterval(VertexHelper vh, IList<UILineInfo> lines)
        {
            UIVertex vertex = default;
            Dictionary<int, float> lineIdx_minX = cache_lineIdx_minX;
            Dictionary<int, float> lineIdx_maxX = cache_lineIdx_maxX;
            for (int i = 0, count = vh.currentVertCount; i < count; i += 4)
            {
                vh.PopulateUIVertex(ref vertex, i);
                var maxY = vertex.position.y;
                var minX = vertex.position.x;
                vh.PopulateUIVertex(ref vertex, i + 2);
                // var minY = vertex.position.y;
                var maxX = vertex.position.x;
                var idx = GetLineIndex(maxY, lines);
                if (!lineIdx_minX.TryGetValue(idx, out var existMinX))
                {
                    lineIdx_minX[idx] = minX;
                }
                else
                {
                    if (existMinX > minX) lineIdx_minX[idx] = minX;
                }

                if (!lineIdx_maxX.TryGetValue(idx, out var existMaxX))
                {
                    lineIdx_maxX[idx] = maxX;
                }
                else
                {
                    if (existMaxX < maxX) lineIdx_maxX[idx] = maxX;
                }
            }

            LineInterval.Clear();
            foreach (var key in lineIdx_minX.Keys)
            {
                LineInterval.Add(key, new Vector2(lineIdx_minX[key], lineIdx_maxX[key]));
            }

            cache_lineIdx_minX.Clear();
            cache_lineIdx_maxX.Clear();
        }

        private Text Text
        {
            get
            {
                if (!_text)
                {
                    _text = GetComponent<Text>();
                }

                return _text;
            }
        }

        private Text _text;

        void IMeshModifier.ModifyMesh(Mesh mesh)
        {
            //do nothing
        }

        private static int GetLineIndex(float value, IList<UILineInfo> lineInfos)
        {
            int idx = 0;
            for (int i = 0, count = lineInfos.Count; i < count; i++)
            {
                if (lineInfos[i].topY > value)
                {
                    idx = i;
                }
                else break;
            }

            return idx;
        }

        void IMeshModifier.ModifyMesh(VertexHelper vh)
        {
            float halfOfTargetWidth = Text.rectTransform.rect.width * 0.5f;
            Vector2 targetLineXRange = new Vector2(-halfOfTargetWidth, halfOfTargetWidth);
            IList<UILineInfo> lines = Text.cachedTextGenerator.lines;
            UpdateLineInterval(vh, lines);
            var lineXRanges = LineInterval;
            UIVertex vertex = default;
            Vector2 ltPos = default;
            for (int i = 0, count = vh.currentVertCount; i < count; i += 2)
            {
                vh.PopulateUIVertex(ref vertex, i);
                var posIdxOfChar = i % 4;
                switch (posIdxOfChar)
                {
                    case 0:
                        ltPos = vertex.position;
                        break;
                    case 2:
                    {
                        var rbPos = vertex.position;
                        var lineIdx = GetLineIndex(ltPos.y, lines);
                        if (!lineXRanges.TryGetValue(lineIdx, out var lineXRange)) continue; //empty line
                        var halfOfCharWidth = (rbPos.x - ltPos.x) * 0.5f;
                        var charPosX = ltPos.x + halfOfCharWidth;
                        var newCharPos = Remap(charPosX,
                            lineXRange.x,
                            lineXRange.y,
                            targetLineXRange[0],
                            targetLineXRange[1]);

                        int j = i - 2;
                        vh.PopulateUIVertex(ref vertex, j);
                        var pos = vertex.position;
                        pos.x = newCharPos - halfOfCharWidth;
                        vertex.position = pos;
                        vh.SetUIVertex(vertex, j);

                        j++;
                        vh.PopulateUIVertex(ref vertex, j);
                        pos = vertex.position;
                        pos.x = newCharPos + halfOfCharWidth;
                        vertex.position = pos;
                        vh.SetUIVertex(vertex, j);

                        j++;
                        vh.PopulateUIVertex(ref vertex, j);
                        pos = vertex.position;
                        pos.x = newCharPos + halfOfCharWidth;
                        vertex.position = pos;
                        vh.SetUIVertex(vertex, j);

                        j++;
                        vh.PopulateUIVertex(ref vertex, j);
                        pos = vertex.position;
                        pos.x = newCharPos - halfOfCharWidth;
                        vertex.position = pos;
                        vh.SetUIVertex(vertex, j);
                    }
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Remap(float v, float vMin, float vMax, float newMin, float newMax)
        {
            return (v - vMin) / (vMax - vMin) * (newMax - newMin) + newMin;
        }
    }
}