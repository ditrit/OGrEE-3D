using UnityEngine;
using System;
using System.Text;
using TMPro;

namespace Udonba
{
    /// <summary>
    /// Debug.LogをTextMeshProに表示
    /// </summary>
    public class LogPrompter : MonoBehaviour
    {
        private TMP_Text _tmpText = null;
        public TMP_Text TmpText
        {
            get
            {
                if (_tmpText == null)
                    _tmpText = this.GetComponent<TextMeshProUGUI>();
                if (_tmpText == null)
                    _tmpText = this.GetComponent<TextMeshPro>(); // TextMeshProUGUIじゃなかったらTextMeshPro
                return _tmpText;
            }
        }

        private StringBuilder _builder = new StringBuilder();

        [SerializeField, Tooltip("テキストの先頭に時刻を表示する")]
        private bool _useTimeStamp = true;

        [SerializeField, Tooltip("ログの種別に応じて色を付ける")]
        private bool _coloredByLogType = true;

        [SerializeField, Tooltip("特定の文字列を含むログは表示しない")]
        private string[] _ignorePhrases = new string[] { };

        private void Awake()
        {
            if (TmpText == null)
            {
                this.enabled = false;
                throw new NullReferenceException("No text component found.");
            }

            Application.logMessageReceived -= HandleLog;
            Application.logMessageReceived += HandleLog;

            _builder = new StringBuilder();

            TmpText.overflowMode = TextOverflowModes.Overflow;

            if (_coloredByLogType)
                TmpText.richText = true;

            TmpText.text = string.Empty;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logText, string stackTrace, LogType logType)
        {
            _builder.Clear();

            // 除外文字列を含むか確認
            if (0 < _ignorePhrases.Length)
            {
                for (int i = 0; i < _ignorePhrases.Length; i++)
                {
                    if (_ignorePhrases[i] != string.Empty && logText.IndexOf(_ignorePhrases[i]) != -1)
                    {
                        return;
                    }
                }
            }

            // タイムスタンプの付加
            if (_useTimeStamp)
            {
                _builder.Append($"[{DateTime.Now.ToLongTimeString()}:{DateTime.Now.Millisecond:D3}] ");
            }

            // 色付け
            if (_coloredByLogType)
            {
                switch (logType)
                {
                    case LogType.Assert:
                    case LogType.Warning:
                        logText = GetColoredString(logText, "yellow");
                        break;
                    case LogType.Error:
                    case LogType.Exception:
                        logText = GetColoredString(logText, "red");
                        break;
                    default:
                        break;
                }
            }

            _builder.AppendLine(logText);
            TmpText.text += _builder.ToString();

            // Textの範囲内に収める
            if (TmpText.overflowMode == TextOverflowModes.Overflow)
            {
                AdjustText(TmpText);
            }
        }

        /// <summary>
        /// 色付き文字列に変換
        /// </summary>
        /// <param name="src"></param>
        /// <param name="colorString"></param>
        /// <returns></returns>
        private string GetColoredString(string src, string colorString)
        {
            return $"<color={colorString}>{src}</color>";
        }

        /// <summary>
        /// Textの範囲内に文字列を収める
        /// </summary>
        /// <param name="tmpText"></param>
        private void AdjustText(TMP_Text tmpText)
        {
            tmpText.ForceMeshUpdate();

            if (tmpText.firstOverflowCharacterIndex == -1)
            {
                return;
            }

            // はみ出した行インデックス
            int overflowLineIdx = -1;
            for (int i = 0; i < tmpText.textInfo.lineCount; i++)
            {
                if (tmpText.firstOverflowCharacterIndex == tmpText.textInfo.lineInfo[i].firstCharacterIndex)
                {
                    overflowLineIdx = i;
                    break;
                }
            }
            if (overflowLineIdx == -1)
            {
                return;
            }

            // はみ出したログ数
            int overflowLogCount = 0;
            for (int i = overflowLineIdx; i < tmpText.textInfo.lineCount; i++)
            {
                for (int j = tmpText.textInfo.lineInfo[i].firstCharacterIndex; j <= tmpText.textInfo.lineInfo[i].lastCharacterIndex; j++)
                {
                    var c = tmpText.textInfo.characterInfo[j].character;
                    if (c == '\n')
                    {
                        overflowLogCount++;
                        break;
                    }

                    if (i == tmpText.textInfo.lineCount - 1 && j == tmpText.textInfo.lineInfo[i].lastCharacterIndex)
                    {
                        overflowLogCount++;
                        break;
                    }
                }
            }

            // 削除したい文字列の長さ
            int deleteLength = 0;
            int foundIdx = 0;
            for (int i = 0; i < overflowLogCount; i++)
            {
                foundIdx = tmpText.text.IndexOf('\n', foundIdx + 1);
            }
            deleteLength = foundIdx + 1; // 末尾の改行コードも消す


            // 削除
            tmpText.text = tmpText.text.Remove(0, deleteLength);
        }
    }
}