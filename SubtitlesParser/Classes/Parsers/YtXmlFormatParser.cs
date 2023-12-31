﻿using SubtitlesParser.Classes.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace SubtitlesParser.Classes.Parsers
{
    public class YtXmlFormatParser : ISubtitlesParser
    {
        public List<SubtitleItem> ParseStream(Stream xmlStream, Encoding encoding)
        {
            Assertions.ValidateStream(xmlStream);

            var items = new List<SubtitleItem>();

            // parse xml stream
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlStream);

            if (xmlDoc.DocumentElement != null)
            {
                var nodeList = xmlDoc.DocumentElement.SelectNodes("//text");

                if (nodeList != null)
                {
                    for (var i = 0; i < nodeList.Count; i++)
                    {
                        var node = nodeList[i];
                        try
                        {
                            var startString = node.Attributes["start"].Value;
                            var start = TimeSpan.FromSeconds(double.Parse(startString, CultureInfo.InvariantCulture));
                            var durString = node.Attributes["dur"].Value;
                            var duration = TimeSpan.FromSeconds(double.Parse(durString, CultureInfo.InvariantCulture));
                            var text = node.InnerText;

                            items.Add(new SubtitleItem()
                            {
                                StartTime = start,
                                EndTime = start + duration,
                                Lines = new List<string>() { text }
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception raised when parsing xml node {0}: {1}", node, ex);
                        }
                    }
                }
            }

            if (items.Any())
            {
                return items;
            }
            else
            {
                throw new ArgumentException("Stream is not in a valid Youtube XML format");
            }
        }
    }
}
