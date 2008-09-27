/*
+---------------------------------------------------------------------------+
|	Demon - dAmn Emulator													|
|===========================================================================|
|	Copyright © 2008 Nol888													|
|===========================================================================|
|	This file is part of Demon.												|
|																			|
|	Demon is free software: you can redistribute it and/or modify			|
|	it under the terms of the GNU Affero General Public License as			|
|	published by the Free Software Foundation, either version 3 of the		|
|	License, or (at your option) any later version.							|
|																			|
|	This program is distributed in the hope that it will be useful,			|
|	but WITHOUT ANY WARRANTY; without even the implied warranty of			|
|	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the			|
|	GNU Affero General Public License for more details.						|
|																			|
|	You should have received a copy of the GNU Affero General Public License|
|	along with this program.  If not, see <http://www.gnu.org/licenses/>.	|
|																			|
|===========================================================================|
|	> $Date$
|	> $Revision$
|	> $Author$
+---------------------------------------------------------------------------+
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace DemonServer
{
	public static class Crypto
	{
		#region Private Properties
		private static Random prng = new Random();

		private static SHA512Managed shaProvider = new SHA512Managed();
		private static MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
		#endregion

		#region Public Methods
		public static string hash(string input, string salt)
		{
			// Get unicode bytes from string and salt.
			System.Text.UnicodeEncoding str = new UnicodeEncoding();
			byte[] inputBytes = str.GetBytes(input);
			byte[] saltBytes = str.GetBytes(salt);

			// Hash once with salt.
			byte[] plainText = new byte[inputBytes.Length + saltBytes.Length];
			Buffer.BlockCopy(inputBytes, 0, plainText, 0, inputBytes.Length);
			Buffer.BlockCopy(saltBytes, 0, plainText, inputBytes.Length, saltBytes.Length);
			byte[] cypherText = Crypto.shaProvider.ComputeHash(plainText);

			// Hash once with firsthash+plain+salt
			byte[] secondPlainText = new byte[cypherText.Length + plainText.Length];
			Buffer.BlockCopy(cypherText, 0, secondPlainText, 0, cypherText.Length);
			Buffer.BlockCopy(plainText, 0, secondPlainText, cypherText.Length, plainText.Length);
			byte[] hash = Crypto.shaProvider.ComputeHash(plainText);

			// Turn hash into hex string.
			string hashStr = "";
			foreach (byte hexByte in hash)
			{
				hashStr += hexByte.ToString("X2");
			}

			return hashStr;
		}

		public static string genSalt()
		{
			return "salt12";
		}
		#endregion
	}
}
