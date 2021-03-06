﻿// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2005-2015, SIL International.
// <copyright from='2005' to='2015' company='SIL International'>
//		Copyright (c) 2005-2015, SIL International.
//    
//		This software is distributed under the MIT License, as specified in the LICENSE.txt file.
// </copyright> 
#endregion
// 
using System.Collections.Generic;

namespace SIL.PaToFdoInterfaces
{
	/// ----------------------------------------------------------------------------------------
	public interface IPaComplexFormInfo
	{
		/// ------------------------------------------------------------------------------------
		List<string> Components { get; }
		
		/// ------------------------------------------------------------------------------------
		IPaMultiString ComplexFormComment { get; }

		/// ------------------------------------------------------------------------------------
		IEnumerable<IPaCmPossibility> ComplexFormType { get; }
	}
}
