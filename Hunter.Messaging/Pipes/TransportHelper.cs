using System;
using System.Text.Json;
using Messaging.Abstractions;

namespace Messaging.Pipes
{
    /// <summary>
    /// Helper methods for building or manipulating <see cref="TransportDTO"/> messages
    /// before sending through the messaging pipelines.
    /// </summary>
    public static class TransportHelper
    {
        /// <summary>
        /// Build a DTO for a simple request.
        /// </summary>
        /// <param name="targetProject">The logical project destination.</param>
        /// <param name="targetFunction">The function/method to invoke.</param>
        /// <param name="arguments">The JSON payload for the call.</param>
        /// <param name="userId">The calling user id, if any.</param>
        /// <param name="fireAndForget">If true, caller does not expect a response.</param>
        /// <returns>A fully populated TransportDTO.</returns>
        public static TransportDTO BuildRequest(
            string targetProject,
            string targetFunction,
            JsonElement arguments,
            int userId,
            bool fireAndForget = false)
        {
            return new TransportDTO
            {
                TargetProject = targetProject,
                TargetFunction = targetFunction,
                Arguments = arguments.GetRawText(), // serialize JSON element into string
                UserId = userId.ToString(),         // FIX: ensure UserId stored as string
                IsFireAndForget = fireAndForget,
                ExpectsResponse = !fireAndForget,
                RequestId = Guid.NewGuid(),
                IsResponse = false
            };
        }

        /// <summary>
        /// Build a DTO for a response.
        /// </summary>
        /// <param name="request">The original request being responded to.</param>
        /// <param name="returnValue">The JSON return value.</param>
        /// <param name="error">Optional error message if the response is failed.</param>
        /// <returns>A populated response DTO ready to send back.</returns>
        public static TransportDTO BuildResponse(
            TransportDTO request,
            JsonElement returnValue,
            string? error = null)
        {
            return new TransportDTO
            {
                TargetProject = request.TargetProject,
                TargetFunction = request.TargetFunction,
                RequestId = request.RequestId,
                IsResponse = true,
                ReturnData = returnValue.GetRawText(), // FIX: store as JSON string
                Error = error,
                UserId = request.UserId,
                Parent = request
            };
        }
    }
}
