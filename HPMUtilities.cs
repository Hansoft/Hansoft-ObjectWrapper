using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using HPMSdk;

namespace Hansoft.ObjectWrapper
{
    /// <summary>
    /// Static utility functions for working with the Hansoft API. 
    /// </summary>
    public static class HPMUtilities
    {

        #region Backlog Priority

        /// <summary>
        /// Sort a list of ProdutcBacklogItems in the same order as they are displayed in the GUI in the priority vuew
        /// </summary>
        /// <param name="project">The Project that the ProductBacklogItems belong to.</param>
        /// <param name="unsorted">The ProductBacklogItems that should be sorted.</param>
        /// <returns></returns>
        public static List<ProductBacklogItem> SortByPriority(Project project, List<ProductBacklogItem> unsorted)
        {
            List<ProductBacklogItem> sorted = new List<ProductBacklogItem>();
            foreach (ProductBacklogItem anItem in unsorted)
            {
                if (anItem is ProductBacklogItemInSprint || anItem is ProductBacklogItemInSchedule)
                    sorted.Add((ProductBacklogItem)Task.GetTask(anItem.Session.TaskGetMainReference(anItem.UniqueTaskID)));
                else
                    sorted.Add(anItem);
            }

            List<HansoftItem> sortedBacklog = new List<HansoftItem>();
            List<HansoftItem> allLeaves = project.ProductBacklog.DeepLeaves;
            ProductBacklogItem item = (ProductBacklogItem)allLeaves.Find(leaf => !allLeaves.Exists(prevLeaf => prevLeaf.Session.TaskRefGetPreviousWorkPriorityID(prevLeaf.UniqueID).m_ID == leaf.UniqueID.m_ID));
            sortedBacklog.Add(item);
            HPMUniqueID nextId = item.Session.TaskRefGetPreviousWorkPriorityID(item.UniqueID);
            while (nextId != -2)
            {
                item = (ProductBacklogItem)Task.GetTask(nextId);
                sortedBacklog.Add(item);
                nextId = item.Session.TaskRefGetPreviousWorkPriorityID(item.UniqueID);
            }
            foreach (ProductBacklogItem aItem in sorted)
                aItem.AbsolutePriority = sortedBacklog.FindIndex(ii => ii.UniqueID.m_ID == aItem.UniqueID.m_ID);

            PriorityComparer comparer = new PriorityComparer();
            sorted.Sort(comparer);
            sorted.Reverse();

            return sorted;
        }

        private class PriorityComparer: IComparer<ProductBacklogItem>
        {
            public int Compare(ProductBacklogItem x, ProductBacklogItem y)
            {
                return x.AbsolutePriority-y.AbsolutePriority;
            }
        }

        #endregion

        #region Project Level

        /// <summary>
        /// Find a project with a specified name in the database that SessionManager is connected to.
        /// </summary>
        /// <param name="projectName">The name of the project to find.</param>
        /// <returns>The found Project or null if not found.</returns>
        public static Project FindProject(string projectName)
        {
            return GetProjects().Find(p => p.Name == projectName);
        }

        /// <summary>
        /// Find projects with names matching the specified regular expression in the database that SessionManager is connected to.
        /// Archived projects will not be included.
        /// </summary>
        /// <param name="regex">The regular expression to match project names against.</param>
        /// <param name="includeArchived">Set to true if archived projects should be included</param>
        /// <returns>The found projects.</returns>
        public static List<Project> FindProjects(string regex, bool inverted)
        {
            return FindProjects(regex, inverted, false);
        }


        /// <summary>
        /// Find projects with names matching the specified regular expression in the database that SessionManager is connected to.
        /// </summary>
        /// <param name="regex">The regular expression to match project names against.</param>
        /// <param name="includeArchived">Set to true if archived projects should be included</param>
        /// <returns>The found projects.</returns>
        public static List<Project> FindProjects(string regex, bool inverted, bool includeArchived)
        {
            List<Project> matches = new List<Project>();
            Regex matcher = new Regex(regex);
            foreach (Project p in GetProjects(includeArchived))
            {
                if (!inverted)
                {
                    if (matcher.IsMatch(p.Name))
                        matches.Add(p);
                }
                else
                {
                    if (!matcher.IsMatch(p.Name))
                        matches.Add(p);
                }
            }
            return matches;
        }

        /// <summary>
        /// The projects in the database that the SessionManager is connected to.
        /// </summary>
        /// <param name="includeArchived">Set to true if archived projects should be included</param>
        /// <returns>The list of projects.</returns>
        public static List<Project> GetProjects(bool includeArchived)
        {
            List<Project> projects = new List<Project>();
            HPMProjectEnum projectIDs = SessionManager.Session.ProjectEnum();
            foreach (HPMUniqueID projId in projectIDs.m_Projects)
            {
                HPMProjectProperties properties = SessionManager.Session.ProjectGetProperties(projId);
                if (!properties.m_bArchivedStatus || includeArchived)
                    projects.Add(Project.GetProject(projId));
            }
            return projects;
        }

        /// <summary>
        /// The projects in the database that the SessionManager is connected to.
        /// </summary>
        /// <returns>The list of projects.</returns>
        public static List<Project> GetProjects()
        {
            List<Project> projects = new List<Project>();
            HPMProjectEnum projectIDs = SessionManager.Session.ProjectEnum();
            foreach (HPMUniqueID projId in projectIDs.m_Projects)
                projects.Add(Project.GetProject(projId));
            return projects;
        }

        /// <summary>
        /// The users in the database that the SessionManager is connected to.
        /// </summary>
        /// <returns>The list of users.</returns>
        public static List<User> GetUsers()
        {
            List<User> users = new List<User>();
            HPMResourceEnum userEnum = SessionManager.Session.ResourceEnum();
            foreach (HPMUniqueID userId in userEnum.m_Resources)
                users.Add(User.GetUser(userId));
            return users;
        }

        /// <summary>
        /// The groups in the database that the SessionManager is connected to.
        /// </summary>
        /// <returns>The list of groups.</returns>
        public static List<Group> GetGroups()
        {
            List<Group> groups = new List<Group>();
            HPMResourceGroupEnum groupEnum = SessionManager.Session.ResourceGroupEnum();
            foreach (HPMUniqueID groupId in groupEnum.m_ResourceGroups)
                groups.Add(Group.GetGroup(groupId));
            return groups;
        }

        #endregion

        #region Date and Time

        private static DateTime t1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Converts a DateTime value to a time value specified as microseconds since Jan 1 1970
        /// </summary>
        /// <param name="utc">The DateTime value (UTC time) to be converted.</param>
        /// <returns>The converted time value specified as microseconds since Jan 1 1970</returns>
        public static UInt64 HPMDateTime(DateTime utc)
        {
            return HPMDateTime(utc, false);
        }

        /// <summary>
        /// Converts a DateTime value to a time value specified as microseconds since Jan 1 1970
        /// </summary>
        /// <param name="utc">The DateTime value (UTC time) to be converted.</param>
        /// <param name="alignedOnWholeDay">True if the converted value should be truncated to midnight, False otherwise.</param>
        /// <returns>The converted time value specified as microseconds since Jan 1 1970</returns>
        public static UInt64 HPMDateTime(DateTime utc, bool alignedOnWholeDay)
        {
            TimeSpan Span = utc - t1970;
            UInt64 microSeconds = (UInt64)(Span.Ticks / 10);
            UInt64 microSecondsInADay = ((ulong)(24 * 60 * 60) * 1000000);
            if (alignedOnWholeDay)
                microSeconds = (microSeconds / microSecondsInADay) * microSecondsInADay;

            return microSeconds;
        }

        /// <summary>
        /// Converts time specified as microseconds since Jan 1 1970 to a DateTime value.
        /// </summary>
        /// <param name="hpmDateTime">Time specified as microseconds since Jan 1 1970</param>
        /// <returns>The converted value</returns>
        public static DateTime FromHPMDateTime(UInt64 hpmDateTime)
        {
            return t1970.AddTicks((long)(hpmDateTime * 10));
        }

        /// <summary>
        /// Current time specified as microseconds since Jan 1 1970.
        /// </summary>
        /// <returns>Current time specified as microseconds since Jan 1 1970.</returns>
        public static UInt64 HPMNow()
        {
            return HPMNow(false);
        }

        /// <summary>
        /// The current time as microseconds since Jan 1 1970.
        /// </summary>
        /// <param name="alignedOnWholeDay">>True if the converted value should be truncated to midnight, False otherwise.</param>
        /// <returns>Current time specified as microseconds since Jan 1 1970.</returns>
        public static UInt64 HPMNow(bool alignedOnWholeDay)
        {
            return HPMDateTime(DateTime.UtcNow, alignedOnWholeDay);
        }

        /// <summary>
        /// Convert a date/time value given as microseconds since Jan 1 1970 to a string with date and time suitable to be displayed to end users.
        /// </summary>
        /// <param name="hpmDateTime">The date/time value to be converted given as microseconds since Jan 1 1970.</param>
        /// <returns>The time value formatted as an end user suitable string.</returns>
        public static string HPMDateTimeToDateTimeString(UInt64 hpmDateTime)
        {
            if (hpmDateTime != 0)
            {
                DateTime dt = FromHPMDateTime(hpmDateTime);
                return dt.ToLocalTime().ToString();
            }
            else
                return "";
        }

        /// <summary>
        /// Convert a date/time value given as microseconds since Jan 1 1970 to a string with date (without time) suitable to be displayed to end users.
        /// </summary>
        /// <param name="hpmDateTime">The date/time value to be converted given as microseconds since Jan 1 1970.</param>
        /// <returns>The time value formatted as an end user suitable string.</returns>
        public static string HPMDateTimeToDateString(UInt64 hpmDateTime)
        {
            DateTime dt = FromHPMDateTime(hpmDateTime);
            return dt.ToLocalTime().ToShortDateString();
        }
        #endregion

        #region Encoding/Decoding Droplist values

        internal static string DecodeDroplistValue(int iVal, HPMProjectCustomColumnsColumnDropListItem[] droplistItem)
        {
            for (int i = 0; i < droplistItem.Length; i += 1)
            {
                if (droplistItem[i].m_Id == iVal)
                    return droplistItem[i].m_Name;
            }
            return string.Empty;
        }

        internal static string DecodeDroplistValues(int[] iVals,  HPMProjectCustomColumnsColumnDropListItem[] droplistItem)
        {
            StringBuilder sb = new StringBuilder();
            for (int i=0; i<iVals.Length; i += 1)
            {
                string sVal = DecodeDroplistValue(iVals[i], droplistItem);
                if (sVal != string.Empty)
                {
                    if (sb.Length > 0)
                        sb.Append(";");
                    sb.Append(sVal);
                }
            }
            return sb.ToString();
        }

        internal static int EncodeDroplistValue(string sVal, HPMProjectCustomColumnsColumnDropListItem[] droplistItem)
        {
            for (int i = 0; i < droplistItem.Length; i += 1)
            {
                if (droplistItem[i].m_Name == sVal)
                    return (int)droplistItem[i].m_Id;
            }
            return -1;

        }

        internal static int[] EncodeDroplistValues(string sVal, HPMProjectCustomColumnsColumnDropListItem[] droplistItem)
        {
            string[] sVals = sVal.Split(new char[]{';'});
            int[] iVals = new int[sVals.Length];
            for (int i=0; i<sVal.Length; i += 1)
                iVals[i] = EncodeDroplistValue(sVals[i], droplistItem);
            return iVals;
        }

        #endregion

        #region Localization

        /// <summary>
        /// Gets a localized string from a Hansoft full translation ID.
        /// 
        /// NOTE: Currently there is no way to select which translation to use but UK English will always be used.
        /// </summary>
        /// <param name="fullId">The full translation ID for the string to get.</param>
        /// <returns>The localized string.</returns>
        public static string GetLocalizedStringForFullId(string fullId)
        {
            uint translationId = SessionManager.Session.LocalizationGetTranslationIDFromFullTranslationID(fullId);
            HPMUntranslatedString untranslated = SessionManager.Session.LocalizationCreateUntranslatedStringFromTranslationID(translationId, null);
            HPMLanguage language = SessionManager.Session.LocalizationGetDefaultLanguage();  // This means that UK English always will be used
            return SessionManager.Session.LocalizationTranslateString(language, untranslated);
        }

        #endregion

        #region Hansoft Rich Text Utilities

        /// <summary>
        /// Converts a string with Hansoft markup to Html.
        /// </summary>
        /// <param name="markup">The string to convert.</param>
        /// <returns>The input string with Hansoft markup converted to the equivalent Html.</returns>
        public static string HansoftMarkupToHtml(string markup)
        {
            return ProcessLineBreaksForHtml(ProcessAllTags(markup, tranformToHtmlInstructions));
        }

        /// <summary>
        /// Strips a string from Hansoft markup.
        /// </summary>
        /// <param name="markup">The string to strip.</param>
        /// <returns>The input string stripped from any Hansoft markup.</returns>
        public static string HansoftMarkupToText(string markup)
        {
            return ProcessAllTags(markup, tranformToTextInstructions);
        }

        delegate string TagProcessor(string aString);

        private static string DeleteTag(string tag)
        {
            return string.Empty;
        }

        private static string PassThroughTagAsLower(string tag)
        {
            return tag.ToLower();
        }

        private static string TrimAngleBrackets(string aString)
        {
            return aString.Trim(new char[] { '<', '>' });
        }

        private static string GetTagParameters(string aTag)
        {
            return TrimAngleBrackets(aTag).Split(new char[] { '=' })[1];
        }

        private static string UrlTagToHtml(string tag)
        {
            return string.Format("<a href=\"{0}\">", GetTagParameters(tag));
        }

        private static string ColorTagToHtml(string tag)
        {
            return string.Format("<span style=\"color:rgb({0})\">", GetTagParameters(tag));
        }

        private static string BgColorTagToHtml(string tag)
        {
            return string.Format("<span style=\"background-color:rgb({0})\">", GetTagParameters(tag));
        }

        struct TagTransformationInstruction
        {
            public string TagName;
            public TagProcessor StartTagProcessor;
            public TagProcessor EndTagProcessor;
        }

        static TagTransformationInstruction[] tranformToTextInstructions = new TagTransformationInstruction[] { 
            new TagTransformationInstruction {TagName = "QUOTE", StartTagProcessor = DeleteTag, EndTagProcessor = DeleteTag },
            new TagTransformationInstruction {TagName = "OL", StartTagProcessor = DeleteTag, EndTagProcessor = DeleteTag },
            new TagTransformationInstruction {TagName = "UL", StartTagProcessor = DeleteTag, EndTagProcessor = DeleteTag },
            new TagTransformationInstruction {TagName = "LI", StartTagProcessor = DeleteTag, EndTagProcessor = DeleteTag },
            new TagTransformationInstruction {TagName = "IMAGE", StartTagProcessor = DeleteTag, EndTagProcessor = null },
            new TagTransformationInstruction {TagName = "URL", StartTagProcessor = DeleteTag, EndTagProcessor = DeleteTag },
            new TagTransformationInstruction {TagName = "COLOR", StartTagProcessor = DeleteTag, EndTagProcessor = DeleteTag },
            new TagTransformationInstruction {TagName = "BGCOLOR", StartTagProcessor = DeleteTag, EndTagProcessor = DeleteTag },
            new TagTransformationInstruction {TagName = "BOLD", StartTagProcessor = DeleteTag, EndTagProcessor = DeleteTag },
            new TagTransformationInstruction {TagName = "UNDERLINE", StartTagProcessor = DeleteTag, EndTagProcessor = DeleteTag },
            new TagTransformationInstruction {TagName = "ITALIC", StartTagProcessor = DeleteTag, EndTagProcessor = DeleteTag }
        };

        static TagTransformationInstruction[] tranformToHtmlInstructions = new TagTransformationInstruction[] { 
            new TagTransformationInstruction {TagName = "QUOTE", StartTagProcessor = delegate(string t){return "<tt>";}, EndTagProcessor = delegate(string t){return "</tt>";} },
            new TagTransformationInstruction {TagName = "OL", StartTagProcessor = PassThroughTagAsLower, EndTagProcessor = PassThroughTagAsLower },
            new TagTransformationInstruction {TagName = "UL", StartTagProcessor = PassThroughTagAsLower, EndTagProcessor = PassThroughTagAsLower },
            new TagTransformationInstruction {TagName = "LI", StartTagProcessor = PassThroughTagAsLower, EndTagProcessor = PassThroughTagAsLower },
            new TagTransformationInstruction {TagName = "IMAGE", StartTagProcessor = DeleteTag, EndTagProcessor = DeleteTag},              // This may not be the optimal thing to do...
            new TagTransformationInstruction {TagName = "URL", StartTagProcessor = UrlTagToHtml, EndTagProcessor = delegate(string t){return "</a>";} },
            new TagTransformationInstruction {TagName = "COLOR", StartTagProcessor = ColorTagToHtml, EndTagProcessor = delegate(string t){return "</span>";} },
            new TagTransformationInstruction {TagName = "BGCOLOR", StartTagProcessor = BgColorTagToHtml, EndTagProcessor = delegate(string t){return "</span>";} },
            new TagTransformationInstruction {TagName = "BOLD", StartTagProcessor = delegate(string t){return "<b>";}, EndTagProcessor = delegate(string t){return "</b>";} },
            new TagTransformationInstruction {TagName = "UNDERLINE", StartTagProcessor = delegate(string t){return "<u>";}, EndTagProcessor = delegate(string t){return "</u>";} },
            new TagTransformationInstruction {TagName = "ITALIC", StartTagProcessor = delegate(string t){return "<i>";}, EndTagProcessor = delegate(string t){return "</i>";} }
        };

        private static string ProcessLineBreaksForHtml(string aString)
        {
            StringBuilder result = new StringBuilder();
            StringReader reader = new StringReader(aString);

            int iChar = reader.Read();
            while (iChar != -1)
            {
                if (iChar == 10)
                {
                    if (reader.Peek() != '<')
                        result.Append("<br/>");
                }
                else
                    result.Append((char)iChar);
                iChar = reader.Read();
            }
            return result.ToString();
        }

        private static string ProcessAllTags(string markup, TagTransformationInstruction[] transformations)
        {
            StringBuilder result = new StringBuilder();
            StringReader reader = new StringReader(markup);
            int iChar = reader.Read();
            while (iChar != -1)
            {
                char ch = (char)iChar;
                if (ch == '<')
                {
                    StringBuilder tag = new StringBuilder();
                    tag.Append(ch);
                    iChar = reader.Read();
                    ch = (char)iChar;
                    while (ch != '>')
                    {
                        tag.Append(ch);
                        iChar = reader.Read();
                        ch = (char)iChar;
                    }
                    tag.Append(ch);
                    string tagAsString = tag.ToString();
                    string tagName;
                    bool isEndTag;
                    string tagContent = tagAsString.Trim(new char[] { '<', '>', '/', ' ' });
                    if (tagAsString.StartsWith("</"))
                    {
                        isEndTag = true;
                        tagName = tagContent;
                    }
                    else
                    {
                        isEndTag = false;
                        tagName = tagContent.Split(new char[] { ' ', '=' })[0];
                    }
                    for (int i = 0; i < transformations.Length; i += 1)
                    {
                        if (transformations[i].TagName == tagName)
                        {
                            if (!isEndTag)
                            {
                                if (transformations[i].StartTagProcessor != null)
                                    result.Append(transformations[i].StartTagProcessor(tagAsString));
                            }
                            else
                            {
                                if (transformations[i].EndTagProcessor != null)
                                    result.Append(transformations[i].EndTagProcessor(tagAsString));
                            }
                            break;
                        }
                    }

                }
                else
                    result.Append(ch);
                iChar = reader.Read();
            }
            return result.ToString();
        }

        #endregion

    }
}
