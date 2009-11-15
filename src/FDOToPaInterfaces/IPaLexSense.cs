// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2009, SIL International. All Rights Reserved.
// <copyright from='2009' to='2009' company='SIL International'>
//		Copyright (c) 2009, SIL International. All Rights Reserved.   
//    
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright> 
#endregion
// 
// File: IPaLexSense.cs
// Responsibility: D. Olson
// 
// <remarks>
// </remarks>
// ---------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace SIL.FdoToPaInterfaces
{
	#region IPaLexSense interface
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// 
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public interface IPaLexSense
	{
		//LexEntry[n].Senses[n].Gloss
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the gloss.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString Gloss { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the type of the sense.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaCmPossibility SenseType { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the status.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaCmPossibility Status { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the part of speech.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaCmPossibility PartOfSpeech { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the definition.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString Definition { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the name of the scientific.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		string ScientificName { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the bibliography.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString Bibliography { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the anthropology note.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString AnthropologyNote { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the discourse note.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString DiscourseNote { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the general note.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString GeneralNote { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the grammar note.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString GrammarNote { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the phonology note.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString PhonologyNote { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the semantics note.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString SemanticsNote { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the sociolinguistics note.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString SociolinguisticsNote { get; }
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the encyclopedic info.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString EncyclopedicInfo { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the restrictions.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IPaMultiString Restrictions { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the source.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		string Source { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the usages.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IEnumerable<IPaCmPossibility> Usages { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the semantic domains.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IEnumerable<IPaCmPossibility> SemanticDomains { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the anthro codes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IEnumerable<IPaCmPossibility> AnthroCodes { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the domain types.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		IEnumerable<IPaCmPossibility> DomainTypes { get; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the import residue.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		string ImportResidue { get; }

		//LexEntry[n].Senses[n].Reversal Entries[n]
	}

	#endregion
}
