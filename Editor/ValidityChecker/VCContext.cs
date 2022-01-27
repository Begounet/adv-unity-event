using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AUE
{
    /// <summary>
    /// Context for <see cref="ValidityChecker"/>
    /// </summary>
    public class VCContext
    {
        /// <summary>
        /// Quantity max of items to treat before yielding
        /// </summary>
        private const int NumWorkItemByFrameAllowed = 2000;

        private Stack<string> _stackTrace = new Stack<string>();
        private int _frameWorkItemCount = 0;

        public string FullStackTraceAsString => string.Join("/", _stackTrace.ToArray());
        public int ProgressId { get; set; }

        public void PushStacktrace(string info) => _stackTrace.Push(info);
        public string PopStackTrace() => _stackTrace.Pop();

        public void LogError(string message)
            => Debug.LogError(message + System.Environment.NewLine + FullStackTraceAsString);

        public void IncreaseFrameWorkItemCount() => ++_frameWorkItemCount;
        public bool HasReachedYieldWork() => _frameWorkItemCount > NumWorkItemByFrameAllowed;
        public void ResetFrameWorkItemCount() => _frameWorkItemCount = 0;

        public void SetProgressionDescription(string description) => Progress.SetDescription(ProgressId, description);
    }
}
