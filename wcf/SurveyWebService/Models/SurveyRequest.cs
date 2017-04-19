﻿//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

namespace SurveyDemoService.Models
{
    /// <summary>
    /// Represents the body of a survey request.
    /// </summary>
    public class SurveyRequest
    {
        /// <summary>
        /// Gets or sets the ID of the survey.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the survey.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the question in the survey.
        /// </summary>
        public int QuestionType { get; set; }

        /// <summary>
        /// Gets or sets the title of the question in the survey.
        /// </summary>
        public string QuestionTitle { get; set; }

        /// <summary>
        /// Gets or sets the choices of the question in the survey. Multiple choices
        /// should be comma delimited.
        /// </summary>
        public string QuestionChoices { get; set; }
    }
}