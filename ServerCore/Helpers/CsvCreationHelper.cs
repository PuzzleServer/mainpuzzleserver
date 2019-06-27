using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;

namespace ServerCore.Helpers
{
    /// <summary>
    /// Helpers for creating CSV files.
    /// </summary>
    public class CsvCreationHelper
    {
        /// <summary>
        /// Create a CSV file for download from the list of records.
        /// </summary>
        /// <param name="records">Records with public properties that are the columns of the CSV.</param>
        /// <param name="fileName">Base file name for the downloaded file.</param>
        /// <returns>A FileContentResult with the CSV file.</returns>
        public static FileContentResult CreateCsvFileResult<T>(List<T> records, string fileName)
        {
            MemoryStream stream = new MemoryStream();

            using (TextWriter textWriter = new StreamWriter(stream))
            {
                CsvWriter csvWriter = new CsvWriter(textWriter);
                csvWriter.WriteRecords(records);
            }

            return new FileContentResult(stream.ToArray(), "text/csv") { FileDownloadName = fileName };
        }
    }
}
