// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace StreamJsonRpc
{
    internal static class JsonRpcServerErrorCode
    {
        /// <summary>
        /// Indicates the RPC call was made but the target threw an exception.
        /// </summary>
        public const long InvocationError = -32000L;

        /// <summary>
        /// No callback object was given to the client but an RPC call was attempted.
        /// </summary>
        public const long NoCallbackObject = -32001L;

        /// <summary>
        /// Execution of the server method was aborted due to a cancellation request from the client.
        /// </summary>
        public const long RequestCanceled = -32800L;
    }
}