﻿// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2005-2015, SIL International.
// <copyright from='2005' to='2015' company='SIL International'>
//		Copyright (c) 2005-2015, SIL International.
//    
//		This software is distributed under the MIT License, as specified in the LICENSE.txt file.
// </copyright> 
#endregion
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using L10NSharp;
using SIL.Pa.Model;

namespace SIL.Pa.PhoneticSearching
{
	/// ----------------------------------------------------------------------------------------
	public class SearchQueryValidator : ValidatorBase
	{
		/// ------------------------------------------------------------------------------------
		public SearchQueryValidator(PaProject project) : base(project)
		{
		}

		/// ------------------------------------------------------------------------------------
		public bool GetIsValid(SearchQuery query)
		{
			try
			{
				Errors.Clear();

				VerifyMatchingSymbolPairs(query.Pattern);

				var pattern = query.SearchItem + "/" + query.PrecedingEnvironment + "_" + query.FollowingEnvironment;
				if (!VerifyGeneralPatternStructure(pattern))
				{
					var error = new SearchQueryValidationError(GetPatternSyntaxErrorMsg());
					Errors.Add(error);
				}

				VerifySearchItem(query.SearchItem);
				VerifyPrecedingEnvironment(query.PrecedingEnvironment);
				VerifyFollowingEnvironment(query.FollowingEnvironment);
				VerifyPhonesAndSymbols(query);

				foreach (var item in new[] { query.SearchItem, query.PrecedingEnvironment, query.FollowingEnvironment })
				{
					VerifyTextInSquareBrackets(item);
					VerifyTextInAngleBrackets(item);
					VerifyDiacriticPlaceholders(item);
					VerifyOneDiacriticPlaceholderPerAndGroup(item);

					VerifyNoEmptyTextBetweenOpenAndCloseSymbols(item, PatternParser.FindInnerMostSquareBracketPairs,
						string.Format(LocalizationManager.GetString("PhoneticSearchingMessages.EmptySquareBracketsMsg",
							"The pattern '{0}' contains at least one set of empty square brackets."), item));

					VerifyNoEmptyTextBetweenOpenAndCloseSymbols(item, PatternParser.FindInnerMostBracesPair,
						string.Format(LocalizationManager.GetString("PhoneticSearchingMessages.EmptyBracesMsg",
							"The pattern '{0}' contains at least one set of empty braces."), item));

					VerifyNoEmptyTextBetweenOpenAndCloseSymbols(item, PatternParser.FindInnerAngleBracketPairs,
						string.Format(LocalizationManager.GetString("PhoneticSearchingMessages.EmptyAngleBracketsMsg",
							"The pattern '{0}' contains at least one set of empty angle brackets."), item));

					VerifyMatchingSymbolPairs(item);
					ValidateOrGroups(item);

					var andGroupValidator = new AndGroupValidator(_project);
					andGroupValidator.Verify(item);
					if (andGroupValidator.HasErrors)
						Errors.AddRange(andGroupValidator.Errors);
				}
			}
			catch (Exception e)
			{
				Errors.Add(SearchQueryValidationError.MakeErrorFromException(e, query.Pattern));
			}
			
			query.Errors.AddRange(Errors);
			return !HasErrors;
		}

		/// ------------------------------------------------------------------------------------
		private void VerifyMatchingSymbolPairs(string item)
		{
			VerifyMatchingOpenAndCloseSymbols(item, '[', ']',
				string.Format(LocalizationManager.GetString("PhoneticSearchingMessages.MismatchedNumberOfBracketsMsg",
				"In the pattern '{0}', the number of open square brackets does not match the number of closed."), item));

			VerifyMatchingOpenAndCloseSymbols(item, '{', '}',
				string.Format(LocalizationManager.GetString("PhoneticSearchingMessages.MismatchedNumberOfBracesMsg",
				"In the pattern '{0}', the number of open braces does not match the number of closed."), item));

			VerifyMatchingOpenAndCloseSymbols(item, '<', '>',
				string.Format(LocalizationManager.GetString("PhoneticSearchingMessages.MismatchedNumberOfAngleBracketsMsg",
				"In the pattern '{0}', the number of open angle brackets does not match the number of closed."), item));

			VerifyMatchingOpenAndCloseSymbols(item, '(', ')',
				string.Format(LocalizationManager.GetString("PhoneticSearchingMessages.MismatchedNumberOfParenthesesMsg",
				"In the pattern '{0}', the number of open parentheses does not match the number of closed."), item));
		}

		/// ------------------------------------------------------------------------------------
		public static string GetPatternSyntaxErrorMsg()
		{
			return string.Format(LocalizationManager.GetString("PhoneticSearchingMessages.PatternSyntaxErrorMsg",
				"The pattern contains a syntax error. The correct format for the pattern is '{0}'"),
				App.kEmptyDiamondPattern);
		}

		/// ------------------------------------------------------------------------------------
		public bool VerifyGeneralPatternStructure(string pattern)
		{
			if (pattern.IndexOf('/') < 0 || pattern.IndexOf('_') < 0)
				return false;

			if (pattern.IndexOf('/') > pattern.IndexOf('_'))
				return false;

			// REVIEW: What if user created a class with a slash in the name?
			if (pattern.Count(c => c == '/') > 1)
				return false;

			// REVIEW: What if user created a class with an underscore in the name?
			if (pattern.Count(c => c == '_') > 1)
				return false;

			return (pattern.Split('/', '_').Length == 3);
		}

		/// ------------------------------------------------------------------------------------
		public void VerifySearchItem(string srchItemPattern)
		{
			if (string.IsNullOrEmpty(srchItemPattern))
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.SearchItemMissingMsg", "You must specify a search item."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsOverview" });
				Errors.Add(error);
			}

			if (StripOutStuffWithValidPlusAndStarSymbols(srchItemPattern).Count(c => "#*+".Contains(c)) > 0)
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.InvalidCharactersInSearchItemMsg",
						"The search item portion of the search pattern contains an illegal symbol. " +
						"The symbols '#', '+' and '*' are not valid in the search item."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsZeroOrMore", "hidSearchPatternsOneOrMore",
					"hidSearchPatternsSpaceOrWrdBoundary", "hidSearchPatternsExamples" });

				Errors.Add(error);
			}
		}

		/// ------------------------------------------------------------------------------------
		public void VerifyPrecedingEnvironment(string precedingEnv)
		{
            precedingEnv = precedingEnv.Replace("NOT", "");
			var envWithoutPlusSymbols = StripOutStuffWithValidPlusAndStarSymbols(precedingEnv);
			if (envWithoutPlusSymbols.Count(c => "#*+".Contains(c)) > 1)
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.InvalidCharactersInPrecedingEnvironmentMsg",
						"The preceding environment portion of the search pattern contains an illegal combination of characters. " +
						"The symbols '#', '+' or '*' are not allowed together in the preceding environment."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsZeroOrMore", "hidSearchPatternsOneOrMore",
					"hidSearchPatternsSpaceOrWrdBoundary", "hidSearchPatternsExamples" });
				
				Errors.Add(error);
			}

			int count = precedingEnv.Count(c => c == '#');
			if (count > 1)
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.TooManyWordBoundarySymbolsInPrecedingEnvironmentMsg",
						"The preceding environment portion of the search pattern contains too many word boundary symbols (#). " +
						"Only one is allowed and it must be at the beginning."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsSpaceOrWrdBoundary", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}

			if (count == 1 && !precedingEnv.StartsWith("#", StringComparison.Ordinal))
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.MisplacedWordBoundarySymbolInPrecedingEnvironmentMsg",
						"The preceding environment portion of the search pattern contains a misplaced word boundary symbol (#). " +
						"It must be at the beginning."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsSpaceOrWrdBoundary", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}

			count = precedingEnv.Count(c => c == '*');
			if (count > 1)
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.TooManyZeroOrMoreSymbolsInPrecedingEnvironmentMsg",
						"The preceding environment portion of the search pattern contains too many 'zero or more' symbols (*). " +
						"Only one is allowed and it must be at the beginning."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsZeroOrMore", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}

            if (count == 1 && !precedingEnv.StartsWith("*", StringComparison.Ordinal))
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.MisplacedZeroOrMoreSymbolInPrecedingEnvironmentMsg",
						"The preceding environment portion of the search pattern contains a misplaced 'zero or more' symbol (*). " +
						"It must be at the beginning."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsZeroOrMore", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}

			count = envWithoutPlusSymbols.Count(c => c == '+');
			if (count > 1)
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.TooManyOneOrMoreSymbolsInPrecedingEnvironmentMsg",
						"The preceding environment portion of the search pattern contains too many 'one or more' symbols (+). " +
						"Only one is allowed and it must be at the beginning."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsOneOrMore", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}

            if (count == 1 && !precedingEnv.StartsWith("+", StringComparison.Ordinal))
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.MisplacedOneOrMoreSymbolInPrecedingEnvironmentMsg",
						"The preceding environment portion of the search pattern contains a misplaced 'one or more' symbol (+). " +
						"It must be at the beginning."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsOneOrMore", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}
		}

		/// ------------------------------------------------------------------------------------
		public void VerifyFollowingEnvironment(string followingEnv)
		{
			var envWithoutPlusSymbols = StripOutStuffWithValidPlusAndStarSymbols(followingEnv);

			if (envWithoutPlusSymbols.Count(c => "#*+".Contains(c)) > 1)
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.InvalidCharactersInFollowingEnvironmentMsg",
						"The following environment portion of the search pattern contains an illegal combination of characters. " +
						"The symbols '#', '+' or '*' are not allowed together in the following environment."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsZeroOrMore", "hidSearchPatternsOneOrMore",
					"hidSearchPatternsSpaceOrWrdBoundary", "hidSearchPatternsExamples" });
				
				Errors.Add(error);
			}

			var count = followingEnv.Count(c => c == '#');
			if (count > 1)
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.TooManyWordBoundarySymbolsInFollowingEnvironmentMsg",
						"The following environment portion of the search pattern contains too many word boundary symbols (#). " +
						"Only one is allowed and it must be at the end."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsSpaceOrWrdBoundary", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}

            if (count == 1 && !followingEnv.EndsWith("#", StringComparison.Ordinal))
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.MisplacedWordBoundarySymbolInFollowingEnvironmentMsg",
						"The following environment portion of the search pattern contains a misplaced word boundary symbol (#). " +
						"It must be at the end."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsSpaceOrWrdBoundary", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}
			count = followingEnv.Count(c => c == '*');
			if (count > 1)
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.TooManyZeroOrMoreSymbolsInFollowingEnvironmentMsg",
						"The following environment portion of the search pattern contains too many zero or more' symbols (*). " +
						"Only one is allowed and it must be at the end."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsZeroOrMore", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}

            if (count == 1 && !followingEnv.EndsWith("*", StringComparison.Ordinal))
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.MisplacedZeroOrMoreSymbolInFollowingEnvironmentMsg",
						"The following environment portion of the search pattern contains a misplaced 'zero or more' symbol (*). " +
						"It must be at the end."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsZeroOrMore", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}

			count = envWithoutPlusSymbols.Count(c => c == '+');
			if (count > 1)
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.TooManyOneOrMoreSymbolsInFollowingEnvironmentMsg",
						"The following environment portion of the search pattern contains too many 'one or more' symbols (+). " +
						"Only one is allowed and it must be at the end."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsOneOrMore", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}

            if (count == 1 && !followingEnv.EndsWith("+", StringComparison.Ordinal))
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.MisplacedOneOrMoreSymbolInFollowingEnvironmentMsg",
						"The following environment portion of the search pattern contains a misplaced 'one or more' symbol (+). " +
						"It must be at the end."));

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsOneOrMore", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}
		}

		/// ------------------------------------------------------------------------------------
		public string StripOutStuffWithValidPlusAndStarSymbols(string pattern)
		{
			var match = PatternParser.FindInnerMostSquareBracketPairs(pattern);
						
			while (match.Success)
			{
				if (match.Value.Contains('+') || match.Value.Contains('*'))
					pattern = pattern.Replace(match.Value, new string('$', match.Value.Length));
				
				match = match.NextMatch();
			}

			return pattern.Replace("$", string.Empty);
		}

		/// ------------------------------------------------------------------------------------
		public void VerifyPhonesAndSymbols(SearchQuery query)
		{
			var phonesNotInCache = GetPhonesNotInCache(query).ToArray();
			if (phonesNotInCache.Length > 0)
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.PatternPhonesNotInDataMsg",
						"The following phone(s) are not found in the data:"));

				foreach (var phone in phonesNotInCache)
					error.PhonesNotInCache.Add(phone);

				Errors.Add(error);
			}

			var symbolsNotInCache = GetSymbolsNotInInventory(query.Pattern).ToArray();
			if (symbolsNotInCache.Length > 0)
			{
				var error = new SearchQueryValidationError(
					LocalizationManager.GetString("PhoneticSearchingMessages.UnknownSymbolsFoundInPatternMsg",
						"The following undefined phonetic symbol(s) were found:"));

				foreach (var symbol in symbolsNotInCache)
					error.SymbolsNotInInventory.Add(symbol);

				error.HelpLinks.AddRange(new[] { "hidSearchPatternsTroubleshooting", "hidTroubleshootingUndefinedPhoneticCharacters" });
				Errors.Add(error);
			}
		}

		/// ------------------------------------------------------------------------------------
		public void VerifyNoEmptyTextBetweenOpenAndCloseSymbols(string pattern,
			Func<string, Match> groupingSymbolRegexProvider, string errorMsg)
		{
			var match = groupingSymbolRegexProvider(pattern);

			while (match.Success)
			{
				var bracketedText = match.Result("${bracketedText}");

				if (bracketedText == string.Empty)
				{
					var error = new SearchQueryValidationError(string.Format(errorMsg, pattern));
					error.HelpLinks.AddRange(new[] { "hidSearchPatternsAndGroups", "hidSearchPatternsOrGroups", "hidSearchPatternsOverview" });
					Errors.Add(error);
					return;
				}

				match = match.NextMatch();
			}
		}

		/// ------------------------------------------------------------------------------------
		public void VerifyMatchingOpenAndCloseSymbols(string pattern, char open, char close,
			string errorMsg)
		{
			if (pattern.Count(c => c == open) != pattern.Count(c => c == close))
			{
				var error = new SearchQueryValidationError(errorMsg);
				error.HelpLinks.AddRange(new[] { "hidSearchPatternsOverview", "hidSearchPatternsExamples" });
				Errors.Add(error);
			}
		}

		/// ------------------------------------------------------------------------------------
		public void VerifyTextInSquareBrackets(string pattern)
		{
			var match = PatternParser.FindInnerMostSquareBracketPairs(pattern);

			while (match.Success)
			{
				var bracketedText = match.Result("${bracketedText}");

				if (bracketedText != string.Empty &&
					!bracketedText.Contains(App.DottedCircle) &&
					bracketedText != "C" && bracketedText != "V" &&
					!App.AFeatureCache.Keys.Any(f => f == bracketedText) &&
					!App.BFeatureCache.Keys.Any(f => f == bracketedText)&&!bracketedText.Contains(App.ProportionalToSymbol))
				{
					var msg = LocalizationManager.GetString("PhoneticSearchingMessages.InvalidTextInSquareBracketsMsg",
						"The text '{0}' in square brackets is invalid. Other than surrounding " +
						"'AND' groups, square brackets are only used to surround descriptive or " +
						"distinctive features, the designators for consonant or vowel classes " +
						"('[C]' and '[V]'), or a diacritic placeholder with its diacritics and wildcards.");

					var error = new SearchQueryValidationError(string.Format(msg, bracketedText));
					error.HelpLinks.AddRange(new[] { "hidSearchPatternsAndGroups", "hidSearchPatternsExamples" });
					Errors.Add(error);
				}

				match = match.NextMatch();
			}
		}

		/// ------------------------------------------------------------------------------------
		public void VerifyTextInAngleBrackets(string pattern)
		{
			var match = PatternParser.FindInnerAngleBracketPairs(pattern);

			while (match.Success)
			{
				var bracketedText = match.Result("${bracketedText}");

				if (!_project.SearchClasses.Any(c => c.Name == bracketedText))
				{
					var msg = LocalizationManager.GetString("PhoneticSearchingMessages.InvalidTextInAngleBracketsMsg",
						"The text '{0}' in angled brackets is not a valid class name.");

					var error = new SearchQueryValidationError(string.Format(msg, bracketedText));
					error.HelpLinks.Add("hidSearchPatternsExamples");
					Errors.Add(error);
				}

				match = match.NextMatch();
			}
		}

		/// ------------------------------------------------------------------------------------
		public void VerifyOneDiacriticPlaceholderPerAndGroup(string pattern)
		{
			var originalPattern = pattern;

			var match = PatternParser.FindInnerMostSquareBracketPairs(pattern);
			while (match.Success)
			{
				var newPattern = pattern.Substring(0, match.Index) +
					(match.Value.Contains(App.DottedCircleC) ? '$' : '@');
				
				newPattern += new string('@', match.Length - 1);
				if (match.Index + match.Length < pattern.Length)
					newPattern += pattern.Substring(match.Index + match.Length);

				pattern = newPattern;
				match = match.NextMatch();
			}

			pattern = pattern.Replace("@", string.Empty);
			match = PatternParser.FindInnerMostSquareBracketPairs(pattern);

			while (match.Success)
			{
				if (match.Value.Count(c => c == '$') > 1)
				{
					var msg = LocalizationManager.GetString("PhoneticSearchingMessages.TooManyDiacriticPlaceholderMsg",
						"The pattern '{0}' contains too many diacritic placeholders in one of the AND groups. " +
						"Only one diacritic placeholder is allowed per AND group.");

					var error = new SearchQueryValidationError(string.Format(msg, originalPattern));
					error.HelpLinks.AddRange(new[] { "hidSearchPatternsDiacriticPlaceholders", "hidSearchPatternsExamples" });
					Errors.Add(error);
					return;
				}

				match = match.NextMatch();
			}
		}
		
		/// ------------------------------------------------------------------------------------
		public void VerifyDiacriticPlaceholders(string pattern)
		{
			var match = PatternParser.FindInnerMostSquareBracketPairs(pattern);
			
			while (match.Success)
			{
				var bracketedText = match.Result("${bracketedText}");
				if (bracketedText.Contains(App.DottedCircleC))
				{
					bracketedText = bracketedText.Replace(App.DottedCircle, string.Empty);
					if (bracketedText.Contains("+") && bracketedText.Contains("*"))
					{
						var error = new SearchQueryValidationError(
							LocalizationManager.GetString("PhoneticSearchingMessages.InvalidDiacriticPlaceholderSyntaxMsg",
								"The symbols '*' and '+' may not appear between square brackets together " +
								"with a diacritic placeholder. One or the other is allowed, but not both."));

						error.HelpLinks.AddRange(new[] { "hidSearchPatternsDiacriticPlaceholders", "hidSearchPatternsExamples" });
						Errors.Add(error);
					}

					if (bracketedText.Count(s => s == '+') > 1)
					{
						var error = new SearchQueryValidationError(
							LocalizationManager.GetString("PhoneticSearchingMessages.TooManyOneOrMoreSymbolsInDiacriticPlaceholderMsg",
								"The One-Or-More-Diacritics symbol (+) appears too many times with a diacritic " +
								"placeholder. Only one is allowed."));

						error.HelpLinks.AddRange(new[] { "hidSearchPatternsDiacriticPlaceholders", "hidSearchPatternsExamples" });
						Errors.Add(error);
					}

					if (bracketedText.Count(s => s == '*') > 1)
					{
						var error = new SearchQueryValidationError(
							LocalizationManager.GetString("PhoneticSearchingMessages.TooManyZeroOrMoreSymbolsInDiacriticPlaceholderMsg",
								"The Zero-Or-More-Diacritics symbol (*) appears too many times with a " +
								"diacritic placeholder. Only one is allowed."));

						error.HelpLinks.AddRange(new[] { "hidSearchPatternsDiacriticPlaceholders", "hidSearchPatternsExamples" });
						Errors.Add(error);
					}

					foreach (var symbolInfo in bracketedText.Where(s => s != '+' && s != '*')
						.Select(s => App.IPASymbolCache[s]).Where(s => s != null && s.IsBase))
					{
						var msg = LocalizationManager.GetString("PhoneticSearchingMessages.InvalidSymbolInDiacriticPlaceholderMsg",
								"The symbol '{0}' is a base character and was found between square brackets " +
								"together with a diacritic placeholder. Base characters are not allowed with " +
								"diacritic placeholders.");

						var error = new SearchQueryValidationError(string.Format(msg, symbolInfo.Literal));
						error.HelpLinks.AddRange(new[] { "hidSearchPatternsDiacriticPlaceholders", "hidSearchPatternsExamples" });
						Errors.Add(error);
					}
				}

				match = match.NextMatch();
			}
		}

		/// ------------------------------------------------------------------------------------
		public void ValidateOrGroups(string item)
		{
			var orGroupValidator = new OrGroupValidator(_project);
			var match = PatternParser.FindInnerMostBracesPair(item);

			while (match.Success)
			{
				orGroupValidator.Verify(match.Value);
				if (orGroupValidator.HasErrors)
					Errors.AddRange(orGroupValidator.Errors);

				match = match.NextMatch();
			}
		}

		#region Methods for getting phones and symbols from pattern
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Checks each character in the query to see if they are in the phonetic character
		/// inventory. If there are some that are invalid, then a list of them is returned.
		///  If the pattern failed to parse, then a SearchQueryException is returned. 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private IEnumerable<char> GetSymbolsNotInInventory(string pattern)
		{
			return GetPhonesRunInPattern(pattern)
				.Where(c => !App.IPASymbolCache.ContainsKey(c))
				.Distinct();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Checks each phone in the pattern to see if it's in the project's phone cache.
		///  A list is made of all phones in the pattern that are not in the cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private IEnumerable<string> GetPhonesNotInCache(SearchQuery query)
		{
			return GetPhonesInPattern(query.Pattern)
				.Where(p => App.IPASymbolCache.ContainsKey(p[0]) &&
					!App.Project.PhoneCache.GetDoesContainPhone(p, query.IgnoredCharacters, query.IgnoreDiacritics))
					.Distinct(StringComparer.Ordinal);

			//return GetPhonesInPattern(pattern)
			//    .Where(p => !App.Project.PhoneCache.ContainsKey(p) && App.IPASymbolCache.ContainsKey(p[0]))
			//    .Distinct(StringComparer.Ordinal);
		}

		/// ------------------------------------------------------------------------------------
		private IEnumerable<string> GetPhonesInPattern(string pattern)
		{
			return (_project.PhoneticParser.Parse(GetPhonesRunInPattern(pattern), true, false) ?? new string[0]);
		}

		/// ------------------------------------------------------------------------------------
		private string GetPhonesRunInPattern(string pattern)
		{
			while (true)
			{
				var match = PatternParser.FindInnerMostParenthesePair(pattern);
				if (!match.Success)
					break;
				pattern = pattern.Replace(match.Value, string.Empty);
			}

			while (true)
			{
				var match = PatternParser.FindInnerMostSquareBracketPairs(pattern);
				if (!match.Success)
					break;
				pattern = pattern.Replace(match.Value, string.Empty);
			}

			while (true)
			{
				var match = PatternParser.FindInnerAngleBracketPairs(pattern);
				if (!match.Success)
					break;
				pattern = pattern.Replace(match.Value, string.Empty);
			}

			pattern = pattern.Replace(",", string.Empty);
			pattern = pattern.Replace("[", string.Empty);
			pattern = pattern.Replace("]", string.Empty);
			pattern = pattern.Replace("{", string.Empty);
			pattern = pattern.Replace("}", string.Empty);
			pattern = pattern.Replace("_", string.Empty);
			pattern = pattern.Replace("/", string.Empty);
			pattern = pattern.Replace("*", string.Empty);
			pattern = pattern.Replace("+", string.Empty);
            pattern = pattern.Replace("NOT", string.Empty);
			return pattern.Replace("#", string.Empty);
		}

		#endregion
	}
}
