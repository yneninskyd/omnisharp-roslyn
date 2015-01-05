﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Text;
using OmniSharp.Models;

namespace OmniSharp
{
    public partial class OmnisharpController
    {
        [HttpPost("autocomplete")]
        public async Task<IActionResult> AutoComplete([FromBody]Request request)
        {
            _workspace.EnsureBufferUpdated(request);

            var completions = new List<AutoCompleteResponse>();

            var documents = _workspace.GetDocuments(request.FileName);

            foreach (var document in documents)
            {
                var sourceText = await document.GetTextAsync();
                var position = sourceText.Lines.GetPosition(new LinePosition(request.Line - 1, request.Column - 1));
                var model = await document.GetSemanticModelAsync();
                var symbols = Recommender.GetRecommendedSymbolsAtPosition(model, position, _workspace);

                completions.AddRange(symbols.Select(MakeAutoCompleteResponse));
            }
            
            return new ObjectResult(completions);
        }

        private AutoCompleteResponse MakeAutoCompleteResponse(ISymbol symbol)
        {
            var response = new AutoCompleteResponse();
            response.CompletionText = symbol.Name;
            // TODO: Do something more intelligent here
            response.DisplayText = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            response.Description = symbol.GetDocumentationCommentXml();

            return response;
        }
    }
}