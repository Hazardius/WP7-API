﻿// DotNetMetroWikiaAPI - Wikia API for C# Metro Style Applications
// Distributed under the terms of the license:
// TODO : NAME THE USED LICENCE
// Authors:
// Hazardius (2012-*) hazardiusam at gmail dot com
//
// Work based on:
// DotNetWikiBot Framework 2.101 - bot framework based on Microsoft .NET Framework 2.0 for wiki projects
// Distributed under the terms of the MIT (X11) license: http://www.opensource.org/licenses/mit-license.php
// Copyright (c) Iaroslav Vassiliev (2006-2012) codedriller@gmail.com
//
// Note: Most time it's just codedriller's code made to work on Windows Phone.

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading;

namespace DotNetMetroWikiaAPI
{
	/// <summary>Class defines wiki site object.</summary>
    public class Site
    {
        /// <summary>Wiki site URL.</summary>
        public string site;
        /// <summary>User's account to login with.</summary>
        public string userName;
        /// <summary>User's password to login with.</summary>
        private string userPass;
        /// <summary>Default domain for LDAP authentication. Additional information can
        /// be found at http://www.mediawiki.org/wiki/Extension:LDAP_Authentication.</summary>
        public string userDomain = "";
        /// <summary>Site title.</summary>
        public string name;
        /// <summary>MediaWiki version as string.</summary>
        public string generator;
        /// <summary>MediaWiki version as number.</summary>
        public float version;
        /// <summary>MediaWiki version as Version object.</summary>
        public Version ver;
        /// <summary>Rule of page title capitalization.</summary>
        public string capitalization;
        /// <summary>Short relative path to wiki pages (if such alias is set on the server).
        /// See "http://www.mediawiki.org/wiki/Manual:Short URL" for details.</summary>
        public string wikiPath;		// = "/wiki/";
        /// <summary>Relative path to "index.php" file on server.</summary>
        public string indexPath;	// = "/w/";
        /// <summary>User's watchlist. Should be loaded manually with FillFromWatchList function,
        /// if it is necessary.</summary>
        public PageList watchList;
        /// <summary>MediaWiki interface messages. Should be loaded manually with
        /// GetMediaWikiMessagesEx function, if it is necessary.</summary>
        public PageList messages;
        /// <summary>Regular expression to find redirection target.</summary>
        public Regex redirectRE;
        /// <summary>Regular expression to find links to pages in list in HTML source.</summary>
        public static Regex linkToPageRE1 =
            new Regex("<li><a href=\"[^\"]*?\" (?:class=\"mw-redirect\" )?title=\"([^\"]+?)\">");
        /// <summary>Alternative regular expression to find links to pages in HTML source.</summary>
        public static Regex linkToPageRE2 =
            new Regex("<a href=\"[^\"]*?\" title=\"([^\"]+?)\">\\1</a>");
        /// <summary>Alternative regular expression to find links to pages (mostly image and file
        /// pages) in HTML source.</summary>
        public Regex linkToPageRE3;
        /// <summary>Regular expression to find links to subcategories in HTML source
        /// of category page on sites that use "CategoryTree" MediaWiki extension.</summary>
        public static Regex linkToSubCategoryRE =
            new Regex(">([^<]+)</a></div>\\s*<div class=\"CategoryTreeChildren\"");
        /// <summary>Regular expression to find links to image pages in galleries
        /// in HTML source.</summary>
        public static Regex linkToImageRE =
            new Regex("<div class=\"gallerytext\">\n<a href=\"[^\"]*?\" title=\"([^\"]+?)\">");
        /// <summary>Regular expression to find titles in markup.</summary>
        public static Regex pageTitleTagRE = new Regex("<title>(.+?)</title>");
        /// <summary>Regular expression to find internal wiki links in markup.</summary>
        public static Regex wikiLinkRE = new Regex(@"\[\[(.+?)(\|.+?)?]]");
        /// <summary>Regular expression to find wiki category links.</summary>
        public Regex wikiCategoryRE;
        /// <summary>Regular expression to find wiki templates in markup.</summary>
        public static Regex wikiTemplateRE = new Regex(@"(?s)\{\{(.+?)((\|.*?)*?)}}");
        /// <summary>Regular expression to find embedded images and files in wiki markup.</summary>
        public Regex wikiImageRE;
        /// <summary>Regular expression to find links to sister wiki projects in markup.</summary>
        public static Regex sisterWikiLinkRE;
        /// <summary>Regular expression to find interwiki links in wiki markup.</summary>
        public static Regex iwikiLinkRE;
        /// <summary>Regular expression to find displayed interwiki links in wiki markup,
        /// like "[[:de:...]]".</summary>
        public static Regex iwikiDispLinkRE;
        /// <summary>Regular expression to find external web links in wiki markup.</summary>
        public static Regex webLinkRE =
            new Regex("(https?|t?ftp|news|nntp|telnet|irc|gopher)://([^\\s'\"<>]+)");
        /// <summary>Regular expression to find sections of text, that are explicitly
        /// marked as non-wiki with special tag.</summary>
        public static Regex noWikiMarkupRE = new Regex("(?is)<nowiki>(.*?)</nowiki>");
        /// <summary>A template for disambiguation page. If some unusual template is used in your
        /// wiki for disambiguation, then it must be set in this variable. Use "|" as a delimiter
        /// when enumerating several templates here.</summary>
        public string disambigStr;
        /// <summary>Regular expression to extract language code from site URL.</summary>
        public static Regex siteLangRE = new Regex(@"https?://(.*?)\.(.+?\..+)");
        /// <summary>Regular expression to extract edit session time attribute.</summary>
        public static Regex editSessionTimeRE1 =
            new Regex("value=\"([^\"]*?)\" name=['\"]wpEdittime['\"]");
        /// <summary>Regular expression to extract edit session time attribute.</summary>
        public static Regex editSessionTimeRE3 = new Regex(" touched=\"(.+?)\"");
        /// <summary>Regular expression to extract edit session token attribute.</summary>
        public static Regex editSessionTokenRE1 =
            new Regex("value=\"([^\"]*?)\" name=['\"]wpEditToken['\"]");
        /// <summary>Regular expression to extract edit session token attribute.</summary>
        public static Regex editSessionTokenRE2 =
            new Regex("name=['\"]wpEditToken['\"](?: type=\"hidden\")? value=\"([^\"]*?)\"");
        /// <summary>Regular expression to extract edit session token attribute.</summary>
        public static Regex editSessionTokenRE3 = new Regex(" edittoken=\"(.+?)\"");
        /// <summary>Site cookies.</summary>
        public CookieContainer cookies = new CookieContainer();
        /// <summary>XML name table for parsing XHTML documents from wiki site.</summary>
        public NameTable xhtmlNameTable = new NameTable();
        /// <summary>XML namespace URI of wiki site's XHTML version.</summary>
        public string xhtmlNSUri = "http://www.w3.org/1999/xhtml";
        /// <summary>XML namespace manager for parsing XHTML documents from wiki site.</summary>
        public XmlNamespaceManager xmlNS;
        /// <summary>Local namespaces.</summary>
        public Dictionary<string, string> namespaces = new Dictionary<string, string>();
        /// <summary>Default namespaces.</summary>
        public static Dictionary<string, string> wikiNSpaces = new Dictionary<string, string>();
        /// <summary>List of Wikimedia Foundation sites and according prefixes.</summary>
        public static Dictionary<string, string> WMSites = new Dictionary<string, string>();
        /// <summary>Built-in variables of MediaWiki software, used in brackets {{...}}.
        /// To be distinguished from templates.
        /// (see http://meta.wikimedia.org/wiki/Help:Magic_words).</summary>
        public static string[] mediaWikiVars;
        /// <summary>Built-in parser functions (and similar prefixes) of MediaWiki software, used
        /// like first ... in {{...:...}}. To be distinguished from templates.
        /// (see http://meta.wikimedia.org/wiki/Help:Magic_words).</summary>
        public static string[] parserFunctions;
        /// <summary>Built-in template modifiers of MediaWiki software
        /// (see http://meta.wikimedia.org/wiki/Help:Magic_words).</summary>
        public static string[] templateModifiers;
        /// <summary>Interwiki links sorting order, based on local language by first word.
        /// See http://meta.wikimedia.org/wiki/Interwiki_sorting_order for details.</summary>
        public static string[] iwikiLinksOrderByLocalFW;
        /// <summary>Interwiki links sorting order, based on local language.
        /// See http://meta.wikimedia.org/wiki/Interwiki_sorting_order for details.</summary>
        public static string[] iwikiLinksOrderByLocal;
        /// <summary>Interwiki links sorting order, based on latin alphabet by first word.
        /// See http://meta.wikimedia.org/wiki/Interwiki_sorting_order for details.</summary>
        public static string[] iwikiLinksOrderByLatinFW;
        /// <summary>Wikimedia Foundation sites and prefixes in one regex-escaped string
        /// with "|" as separator.</summary>
        public static string WMSitesStr;
        /// <summary>ISO 639-1 language codes, used as prefixes to identify Wikimedia
        /// Foundation sites, gathered in one regex-escaped string with "|" as separator.</summary>
        public static string WMLangsStr;
        /// <summary>Availability of "api.php" MediaWiki extension (user interface).</summary>
        public bool userQuery;
        /// <summary>Versions of "api.php" MediaWiki extension (user interface) modules.</summary>
        public Dictionary<string, string> userQueryVersions = new Dictionary<string, string>();
        /// <summary>Set of lists of pages, produced by user interface.</summary>
        public static Dictionary<string, string> userQueryLists = new Dictionary<string, string>();
        /// <summary>Set of lists of parsed data, produced by user interface.</summary>
        public static Dictionary<string, string> userQueryProps = new Dictionary<string, string>();
        /// <summary>Site language.</summary>
        public string language;
        /// <summary>Site language text direction.</summary>
        public string langDirection;
        /// <summary>Site's neutral (language) culture.</summary>
        public CultureInfo langCulture;
        /// <summary>Randomly chosen regional (non-neutral) culture for site's language.</summary>
        public CultureInfo regCulture;
        /// <summary>Windows Phone Isolated Storage</summary>
        private IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

        /// <summary>This constructor is used to generate most Site objects.</summary>
        /// <param name="site">Wiki site's URI. It must point to the main page of the wiki, e.g.
        /// "http://en.wikipedia.org" or "http://127.0.0.1:80/w/index.php?title=Main_page".</param>
        /// <param name="userName">User name to log in.</param>
        /// <param name="userPass">Password.</param>
        /// <returns>Returns Site object.</returns>
        public Site(string site, string userName, string userPass)
            : this(site, userName, userPass, "") { }

        /// <summary>This constructor is used for LDAP authentication. Additional information can
        /// be found at "http://www.mediawiki.org/wiki/Extension:LDAP_Authentication".</summary>
        /// <param name="site">Wiki site's URI. It must point to the main page of the wiki, e.g.
        /// "http://en.wikipedia.org" or "http://127.0.0.1:80/w/index.php?title=Main_page".</param>
        /// <param name="userName">User name to log in.</param>
        /// <param name="userPass">Password.</param>
        /// <param name="userDomain">Domain for LDAP authentication.</param>
        /// <returns>Returns Site object.</returns>
        public Site(string site, string userName, string userPass, string userDomain)
        {
            this.site = site;
            this.userName = userName;
            this.userPass = userPass;
            this.userDomain = userDomain;
            Initialize();
        }

        /// <summary>This constructor uses default site, userName and password. The site URL and
        /// account data can be stored in UTF8-encoded "Defaults.dat" file in user's "Cache"
        /// subdirectory.</summary>
        /// <returns>Returns Site object.</returns>
        public Site()
        {
            if (isf.FileExists("Cache" + System.IO.Path.DirectorySeparatorChar + "Defaults.dat") == true)
            {
                string[] lines = ReadAllLines(
                    "Cache" + System.IO.Path.DirectorySeparatorChar + "Defaults.dat", Encoding.UTF8);
                if (lines.GetUpperBound(0) >= 2)
                {
                    this.site = lines[0];
                    this.userName = lines[1];
                    this.userPass = lines[2];
                    if (lines.GetUpperBound(0) >= 3)
                        this.userDomain = lines[3];
                    else
                        this.userDomain = "";
                }
                else
                    throw new WikiUserException(
                        User.Msg("\"\\Cache\\Defaults.dat\" file is invalid."));
            }
            else
                throw new WikiUserException(User.Msg("\"\\Cache\\Defaults.dat\" file not found."));
            Initialize();
        }

        /// <summary>This internal function establishes connection to site and loads general site
        /// info by the use of other functions. Function is called from the constructors.</summary>
        public void Initialize()
        {
            xmlNS = new XmlNamespaceManager(xhtmlNameTable);
            if (site.Contains("sourceforge"))
            {
                site = site.Replace("https://", "http://");
                GetPaths();
                xmlNS.AddNamespace("ns", xhtmlNSUri);
                LoadDefaults();
                LogInSourceForge();
                site = site.Replace("http://", "https://");
            }
            else if (site.Contains("wikia.com"))
            {
                GetPaths();
                xmlNS.AddNamespace("ns", xhtmlNSUri);
                LoadDefaults();
                LogInViaApi();
            }
            else
            {
                GetPaths();
                xmlNS.AddNamespace("ns", xhtmlNSUri);
                LoadDefaults();
                LogIn();
            }
            GetInfo();
            if (!User.isRunningOnMono)
                User.DisableCanonicalizingUriAsFilePath();	// .NET bug evasion
        }

        private static void asyncHelper(IAsyncResult asyncReceive)
        {
            return;
        }

        /// <summary>Gets path to "index.php", short path to pages (if present), and then
        /// saves paths to file.</summary>
        public void GetPaths()
        {
            if (!site.StartsWith("http"))
                site = "http://" + site;
            if (User.CountMatches(site, "/", false) == 3 && site.EndsWith("/"))
                site = site.Substring(0, site.Length - 1);
            string filePathName = "Cache" + System.IO.Path.DirectorySeparatorChar +
                HttpUtility.UrlEncode(site.Replace("://", ".").Replace("/", ".")) + ".dat";
            if (File.Exists(filePathName) == true)
            {
                string[] lines = ReadAllLines(filePathName, Encoding.UTF8);
                if (lines.GetUpperBound(0) >= 4)
                {
                    wikiPath = lines[0];
                    indexPath = lines[1];
                    xhtmlNSUri = lines[2];
                    language = lines[3];
                    langDirection = lines[4];
                    if (lines.GetUpperBound(0) >= 5)
                        site = lines[5];
                    return;
                }
            }
            Console.WriteLine(User.Msg("Logging in..."));
            //WebClient webClient = new WebClient();
            //webClient.BaseAddress = site;
            //webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
            //webClient.UseDefaultCredentials = true;
            //webReq.ContentType = User.webContentType;
            //webReq.UserAgent = User.userVer;
            string data;
            if (User.unsafeHttpHeaderParsingUsed == 0)
            {
                //webReq.ProtocolVersion = HttpVersion.Version10;
                //webReq.KeepAlive = false;
            }
            for (int errorCounter = 0; true; errorCounter++)
            {
                try
                {
                    data = HTTPGet();

                    //webClient.OpenReadAsync(webResp);
                    //webClient.DownloadStringAsync(webResp);
                    //webResp = (HttpWebResponse)webReq.GetResponse();
                    break;
                }
                catch (WebException e)
                {
                    string message = e.Message;
                    if (Regex.IsMatch(message, ": \\(50[02349]\\) "))
                    {		// Remote problem
                        if (errorCounter > User.retryTimes)
                            throw;
                        Console.Error.WriteLine(message + " " + User.Msg("Retrying in 60 seconds."));
                        Thread.Sleep(60000);
                    }
                    else if (message.Contains("Section=ResponseStatusLine"))
                    {	// Squid problem
                        User.SwitchUnsafeHttpHeaderParsing(true);
                        GetPaths();
                        return;
                    }
                    else
                    {
                        Console.Error.WriteLine(User.Msg("Can't access the site.") + " " + message);
                        throw;
                    }
                }
            }
            //site = webResp.ResponseUri.Scheme + "://" + webResp.ResponseUri.Authority;
            Regex wikiPathRE = new Regex("(?i)" + Regex.Escape(site) + "(/.+?/).+");
            Regex indexPathRE1 = new Regex("(?i)" + Regex.Escape(site) +
                "(/.+?/)index\\.php(\\?|/)");
            Regex indexPathRE2 = new Regex("(?i)href=\"(/[^\"\\s<>?]*?)index\\.php(\\?|/)");
            Regex indexPathRE3 = new Regex("(?i)wgScript=\"(/[^\"\\s<>?]*?)index\\.php");
            Regex xhtmlNSUriRE = new Regex("(?i)<html[^>]*( xmlns=\"(?'xmlns'[^\"]+)\")[^>]*>");
            Regex languageRE = new Regex("(?i)<html[^>]*( lang=\"(?'lang'[^\"]+)\")[^>]*>");
            Regex langDirectionRE = new Regex("(?i)<html[^>]*( dir=\"(?'dir'[^\"]+)\")[^>]*>");
            string mainPageUri = data;
            if (mainPageUri.Contains("/index.php?"))
                indexPath = indexPathRE1.Match(mainPageUri).Groups[1].ToString();
            else
                wikiPath = wikiPathRE.Match(mainPageUri).Groups[1].ToString();
            if (string.IsNullOrEmpty(indexPath) && string.IsNullOrEmpty(wikiPath) &&
                mainPageUri[mainPageUri.Length - 1] != '/' &&
                User.CountMatches(mainPageUri, "/", false) == 3)
                wikiPath = "/";
            // Comment: Hope it won't broke here.
            //Stream respStream = data;
            //if (webResp.ContentEncoding.ToLower().Contains("gzip"))
            //    respStream = new GZipStream(respStream, CompressionMode.Decompress);
            //else if (webResp.ContentEncoding.ToLower().Contains("deflate"))
            //    respStream = new DeflateStream(respStream, CompressionMode.Decompress);
            //StreamReader strmReader = new StreamReader(respStream, Encoding.UTF8);
            string src = data; //strmReader.ReadToEnd();
            if (!site.Contains("wikia.com"))
                indexPath = indexPathRE2.Match(src).Groups[1].ToString();
            else
                indexPath = indexPathRE3.Match(src).Groups[1].ToString();
            xhtmlNSUri = xhtmlNSUriRE.Match(src).Groups["xmlns"].ToString();
            if (string.IsNullOrEmpty(xhtmlNSUri))
                xhtmlNSUri = "http://www.w3.org/1999/xhtml";
            language = languageRE.Match(src).Groups["lang"].ToString();
            langDirection = langDirectionRE.Match(src).Groups["dir"].ToString();
            if (!Directory.Exists("Cache"))
                Directory.CreateDirectory("Cache");
            WriteAllText(filePathName, wikiPath + "\r\n" + indexPath + "\r\n" + xhtmlNSUri +
                "\r\n" + language + "\r\n" + langDirection + "\r\n" + site, Encoding.UTF8);
        }

        /// <summary>Retrieves metadata and local namespace names from site.</summary>
        public void GetInfo()
        {
            try
            {
                langCulture = new CultureInfo(language); //language, false);
            }
            catch (Exception)
            {
                langCulture = new CultureInfo("");
            }
            if (langCulture.Equals(CultureInfo.CurrentUICulture.Parent))
                regCulture = CultureInfo.CurrentUICulture;
            else
            {
                try
                {
                    regCulture = (new CultureInfo(language));
                    //regCulture = CultureInfo.CreateSpecificCulture(language);
                }
                catch (Exception)
                {
                    foreach (CultureInfo ci in 
                        CultureInfo.GetCultures(CultureTypes.SpecificCultures))
                    {
                        if (langCulture.Equals(ci.Parent))
                        {
                            regCulture = ci;
                            break;
                        }
                    }
                    if (regCulture == null)
                        regCulture = CultureInfo.InvariantCulture;
                }
            }

            string src = HTTPGet(site + indexPath + "index.php?title=Special:Export/" +
                DateTime.Now.Ticks.ToString("x"));
            XmlReader reader = XmlReader.Create(new StringReader(src));
            //XmlTextReader reader = new XmlTextReader(new StringReader(src));
            reader.Settings.IgnoreWhitespace = true;
            reader.ReadToFollowing("sitename");
            name = reader.ReadContentAsString();
            reader.ReadToFollowing("generator");
            generator = reader.ReadContentAsString();
            ver = new Version(Regex.Replace(generator, @"[^\d\.]", ""));
            float.TryParse(ver.ToString(), NumberStyles.AllowDecimalPoint,
                new CultureInfo("en-US"), out version);
            reader.ReadToFollowing("case");
            capitalization = reader.ReadContentAsString();
            namespaces.Clear();
            while (reader.ReadToFollowing("namespace"))
                namespaces.Add(reader.GetAttribute("key"),
                    HttpUtility.HtmlDecode(reader.ReadContentAsString()));
            reader.Close();
            namespaces.Remove("0");
            foreach (KeyValuePair<string, string> ns in namespaces)
            {
                if (!wikiNSpaces.ContainsKey(ns.Key) ||
                    ns.Key.ToString() == "4" || ns.Key.ToString() == "5")
                    wikiNSpaces[ns.Key] = ns.Value;
            }
            if (ver >= new Version(1, 14))
            {
                wikiNSpaces["6"] = "File";
                wikiNSpaces["7"] = "File talk";
            }
            wikiCategoryRE = new Regex(@"\[\[(?i)(((" + Regex.Escape(wikiNSpaces["14"].ToString()) +
                "|" + Regex.Escape(namespaces["14"].ToString()) + @"):(.+?))(\|(.+?))?)]]");
            wikiImageRE = new Regex(@"\[\[(?i)((File|Image" +
                "|" + Regex.Escape(namespaces["6"].ToString()) + @"):(.+?))(\|(.+?))*?]]");
            string namespacesStr = "";
            foreach (KeyValuePair<string, string> ns in namespaces)
                namespacesStr += Regex.Escape(ns.Value.ToString()) + "|";
            namespacesStr = namespacesStr.Replace("||", "|").Trim("|".ToCharArray());
            linkToPageRE3 = new Regex("<a href=\"[^\"]*?\" title=\"(" +
                Regex.Escape(namespaces["6"].ToString()) + ":[^\"]+?)\">");
            string redirectTag = "REDIRECT";
            switch (language)
            {		// Revised 2010-07-02 (MediaWiki 1.15.4)
                case "af": redirectTag += "|aanstuur"; break;
                case "ar": redirectTag += "|تحويل"; break;
                case "arz": redirectTag += "|تحويل|تحويل#"; break;
                case "be": redirectTag += "|перанакіраваньне"; break;
                case "be-x-old": redirectTag += "|перанакіраваньне"; break;
                case "bg": redirectTag += "|пренасочване|виж"; break;
                case "br": redirectTag += "|adkas"; break;
                case "bs": redirectTag += "|preusmjeri"; break;
                case "cs": redirectTag += "|přesměruj"; break;
                case "cu": redirectTag += "|прѣнаправлєниѥ"; break;
                case "cy": redirectTag += "|ail-cyfeirio|ailgyfeirio"; break;
                case "de": redirectTag += "|weiterleitung"; break;
                case "el": redirectTag += "|ανακατευθυνση"; break;
                case "eo": redirectTag += "|alidirektu"; break;
                case "es": redirectTag += "|redireccíon"; break;
                case "et": redirectTag += "|suuna"; break;
                case "eu": redirectTag += "|birzuzendu"; break;
                case "fa": redirectTag += "|تغییرمسیر"; break;
                case "fi": redirectTag += "|uudelleenohjaus|ohjaus"; break;
                case "fr": redirectTag += "|redirection"; break;
                case "ga": redirectTag += "|athsheoladh"; break;
                case "gl": redirectTag += "|redirección"; break;
                case "he": redirectTag += "|הפניה"; break;
                case "hr": redirectTag += "|preusmjeri"; break;
                case "hu": redirectTag += "|átirányítás"; break;
                case "hy": redirectTag += "|վերահղում"; break;
                case "id": redirectTag += "|alih"; break;
                case "is": redirectTag += "|tilvísun"; break;
                case "it": redirectTag += "|redirezione"; break;
                case "ja": redirectTag += "|転送|リダイレクト|転送|リダイレクト"; break;
                case "ka": redirectTag += "|გადამისამართება"; break;
                case "kk": redirectTag += "|ايداۋ|айдау|aýdaw"; break;
                case "km": redirectTag += "|បញ្ជូនបន្ត|ប្ដូរទីតាំងទៅ #ប្តូរទីតាំងទៅ"
                    + "|ប្ដូរទីតាំង|ប្តូរទីតាំង|ប្ដូរចំណងជើង"; break;
                case "ko": redirectTag += "|넘겨주기"; break;
                case "ksh": redirectTag += "|ömleidung"; break;
                case "lt": redirectTag += "|peradresavimas"; break;
                case "mk": redirectTag += "|пренасочување|види"; break;
                case "ml": redirectTag += "|аґ¤аґїаґ°аґїаґљаµЌаґљаµЃаґµаґїаґџаµЃаґ•" +
                    "|аґ¤аґїаґ°аґїаґљаµЌаґљаµЃаґµаґїаґџаґІаµЌвЂЌ"; break;
                case "mr": redirectTag += "|а¤ЄаҐЃа¤Ёа¤°аҐЌа¤Ёа¤їа¤°аҐЌа¤¦аҐ‡а¤¶а¤Ё"; break;
                case "mt": redirectTag += "|rindirizza"; break;
                case "mwl": redirectTag += "|ancaminar"; break;
                case "nds": redirectTag += "|wiederleiden"; break;
                case "nds-nl": redirectTag += "|deurverwiezing|doorverwijzing"; break;
                case "nl": redirectTag += "|doorverwijzing"; break;
                case "nn": redirectTag += "|omdiriger"; break;
                case "oc": redirectTag += "|redireccion"; break;
                case "pl": redirectTag += "|patrz|przekieruj|tam"; break;
                case "pt": redirectTag += "|redirecionamento"; break;
                case "ro": redirectTag += "|redirecteaza"; break;
                case "ru": redirectTag += "|перенаправление|перенапр"; break;
                case "sa": redirectTag += "|а¤ЄаҐЃа¤Ёа¤°аҐЌа¤Ёа¤їа¤¦аҐ‡а¤¶а¤Ё"; break;
                case "sd": redirectTag += "|چوريو"; break;
                case "si": redirectTag += "|а¶єа·…а·’а¶єа·ња¶ёа·”а·Ђ"; break;
                case "sk": redirectTag += "|presmeruj"; break;
                case "sl": redirectTag += "|preusmeritev"; break;
                case "sq": redirectTag += "|ridrejto"; break;
                case "sr": redirectTag += "|преусмери|preusmeri"; break;
                case "srn": redirectTag += "|doorverwijzing"; break;
                case "sv": redirectTag += "|omdirigering"; break;
                case "ta": redirectTag += "|а®µа®ґа®їа®®а®ѕа®±аЇЌа®±аЇЃ"; break;
                case "te": redirectTag += "|а°¦а°ѕа°°а°їа°®а°ѕа°°а±Ќа°Єа±Ѓ"; break;
                case "tr": redirectTag += "|yönlendİrme"; break;
                case "tt": redirectTag += "перенаправление|перенапр|yünältü"; break;
                case "uk": redirectTag += "|перенаправлення|перенаправление|перенапр"; break;
                case "vi": redirectTag += "|đổi|đổi"; break;
                case "vro": redirectTag += "|saadaq|suuna"; break;
                case "yi": redirectTag += "|ווייטערפירן|#הפניה"; break;
                default: redirectTag = "REDIRECT"; break;
            }
            redirectRE = new Regex(@"(?i)^#(?:" + redirectTag + @")\s*:?\s*\[\[(.+?)(\|.+)?]]",
                RegexOptions.Compiled);
            Console.WriteLine(User.Msg("Site: {0} ({1})"), name, generator);
            string userQueryUriStr = site + indexPath + "api.php?version";
            string respStr;
            try
            {
                respStr = HTTPGet(userQueryUriStr);
                if (respStr.Contains("<title>MediaWiki API</title>"))
                {
                    userQuery = true;
                    Regex userQueryVersionsRE = new Regex(@"(?i)<b><i>\$" +
                        @"Id: (\S+) (\d+) (.+?) \$</i></b>");
                    foreach (Match m in userQueryVersionsRE.Matches(respStr))
                        userQueryVersions[m.Groups[1].ToString()] = m.Groups[2].ToString();
                    if (!userQueryVersions.ContainsKey("ApiMain.php") && ver > new Version(1, 17))
                    {
                        // if versioning system is broken
                        userQueryVersions["ApiQueryCategoryMembers.php"] = "104449";
                        userQueryVersions["ApiQueryRevisions.php"] = "104449";
                    }
                }
            }
            catch (WebException)
            {
                userQuery = false;
            }
            if ((userQuery == false || !userQueryVersions.ContainsKey("ApiQueryCategoryMembers.php"))
                && ver < new Version(1, 16))
            {
                userQueryUriStr = site + indexPath + "query.php";
                try
                {
                    respStr = HTTPGet(userQueryUriStr);
                    if (respStr.Contains("<title>MediaWiki Query Interface</title>"))
                    {
                        userQuery = true;
                        userQueryVersions["query.php"] = "Unknown";
                    }
                }
                catch (WebException)
                {
                    return;
                }
            }
        }

        /// <summary>Loads default English namespace names for site.</summary>
        public void LoadDefaults()
        {
            if (wikiNSpaces.Count != 0 && WMSites.Count != 0)
                return;

            string[] wikiNSNames = { "Media", "Special", "", "Talk", "User", "User talk", name,
				name + " talk", "Image", "Image talk", "MediaWiki", "MediaWiki talk", "Template",
				"Template talk", "Help", "Help talk", "Category", "Category talk" };
            for (int i = -2, j = 0; i < 16; i++, j++)
                wikiNSpaces.Add(i.ToString(), wikiNSNames[j]);
            wikiNSpaces.Remove("0");

            WMSites.Add("w", "wikipedia"); WMSites.Add("wikt", "wiktionary");
            WMSites.Add("b", "wikibooks"); WMSites.Add("n", "wikinews");
            WMSites.Add("q", "wikiquote"); WMSites.Add("s", "wikisource");
            foreach (KeyValuePair<string, string> s in WMSites)
                WMSitesStr += s.Key + "|" + s.Value + "|";

            // Revised 2010-07-02
            mediaWikiVars = new string[] { "currentmonth","currentmonthname","currentmonthnamegen",
				"currentmonthabbrev","currentday2","currentdayname","currentyear","currenttime",
				"currenthour","localmonth","localmonthname","localmonthnamegen","localmonthabbrev",
				"localday","localday2","localdayname","localyear","localtime","localhour",
				"numberofarticles","numberoffiles","sitename","server","servername","scriptpath",
				"pagename","pagenamee","fullpagename","fullpagenamee","namespace","namespacee",
				"currentweek","currentdow","localweek","localdow","revisionid","revisionday",
				"revisionday2","revisionmonth","revisionyear","revisiontimestamp","subpagename",
				"subpagenamee","talkspace","talkspacee","subjectspace","dirmark","directionmark",
				"subjectspacee","talkpagename","talkpagenamee","subjectpagename","subjectpagenamee",
				"numberofusers","rawsuffix","newsectionlink","numberofpages","currentversion",
				"basepagename","basepagenamee","urlencode","currenttimestamp","localtimestamp",
				"directionmark","language","contentlanguage","pagesinnamespace","numberofadmins",
				"currentday","numberofarticles:r","numberofpages:r","magicnumber",
				"numberoffiles:r", "numberofusers:r", "numberofadmins:r", "numberofactiveusers",
				"numberofactiveusers:r" };
            parserFunctions = new string[] { "ns:", "localurl:", "localurle:", "urlencode:",
				"anchorencode:", "fullurl:", "fullurle:",  "grammar:", "plural:", "lc:", "lcfirst:",
				"uc:", "ucfirst:", "formatnum:", "padleft:", "padright:", "#language:",
				"displaytitle:", "defaultsort:", "#if:", "#if:", "#switch:", "#ifexpr:",
				"numberingroup:", "pagesinns:", "pagesincat:", "pagesincategory:", "pagesize:",
				"gender:", "filepath:", "#special:", "#tag:" };
            templateModifiers = new string[] { ":", "int:", "msg:", "msgnw:", "raw:", "subst:" };
            // Revised 2010-07-02
            iwikiLinksOrderByLocalFW = new string[] {
				"ace", "af", "ak", "als", "am", "ang", "ab", "ar", "an", "arc",
				"roa-rup", "frp", "as", "ast", "gn", "av", "ay", "az", "id", "ms",
				"bm", "bn", "zh-min-nan", "nan", "map-bms", "jv", "su", "ba", "be",
				"be-x-old", "bh", "bcl", "bi", "bar", "bo", "bs", "br", "bug", "bg",
				"bxr", "ca", "ceb", "cv", "cs", "ch", "cbk-zam", "ny", "sn", "tum",
				"cho", "co", "cy", "da", "dk", "pdc", "de", "dv", "nv", "dsb", "na",
				"dz", "mh", "et", "el", "eml", "en", "myv", "es", "eo", "ext", "eu",
				"ee", "fa", "hif", "fo", "fr", "fy", "ff", "fur", "ga", "gv", "sm",
				"gd", "gl", "gan", "ki", "glk", "gu", "got", "hak", "xal", "ko",
				"ha", "haw", "hy", "hi", "ho", "hsb", "hr", "io", "ig", "ilo",
				"bpy", "ia", "ie", "iu", "ik", "os", "xh", "zu", "is", "it", "he",
				"kl", "kn", "kr", "pam", "ka", "ks", "csb", "kk", "kw", "rw", "ky",
				"rn", "sw", "kv", "kg", "ht", "ku", "kj", "lad", "lbe", "lo", "la",
				"lv", "to", "lb", "lt", "lij", "li", "ln", "jbo", "lg", "lmo", "hu",
				"mk", "mg", "ml", "krc", "mt", "mi", "mr", "arz", "mzn", "cdo",
				"mwl", "mdf", "mo", "mn", "mus", "my", "nah", "fj", "nl", "nds-nl",
				"cr", "ne", "new", "ja", "nap", "ce", "pih", "no", "nb", "nn",
				"nrm", "nov", "ii", "oc", "mhr", "or", "om", "ng", "hz", "uz", "pa",
				"pi", "pag", "pnb", "pap", "ps", "km", "pcd", "pms", "nds", "pl",
				"pnt", "pt", "aa", "kaa", "crh", "ty", "ksh", "ro", "rmy", "rm",
				"qu", "ru", "sah", "se", "sa", "sg", "sc", "sco", "stq", "st", "tn",
				"sq", "scn", "si", "simple", "sd", "ss", "sk", "sl", "cu", "szl",
				"so", "ckb", "srn", "sr", "sh", "fi", "sv", "tl", "ta", "kab",
				"roa-tara", "tt", "te", "tet", "th", "vi", "ti", "tg", "tpi",
				"tokipona", "tp", "chr", "chy", "ve", "tr", "tk", "tw", "udm", "uk",
				"ur", "ug", "za", "vec", "vo", "fiu-vro", "wa", "zh-classical",
				"vls", "war", "wo", "wuu", "ts", "yi", "yo", "zh-yue", "diq", "zea",
				"bat-smg", "zh", "zh-tw", "zh-cn"
			};
            iwikiLinksOrderByLocal = new string[] {
				"ace", "af", "ak", "als", "am", "ang", "ab", "ar", "an", "arc",
				"roa-rup", "frp", "as", "ast", "gn", "av", "ay", "az", "bm", "bn",
				"zh-min-nan", "nan", "map-bms", "ba", "be", "be-x-old", "bh", "bcl",
				"bi", "bar", "bo", "bs", "br", "bg", "bxr", "ca", "cv", "ceb", "cs",
				"ch", "cbk-zam", "ny", "sn", "tum", "cho", "co", "cy", "da", "dk",
				"pdc", "de", "dv", "nv", "dsb", "dz", "mh", "et", "el", "eml", "en",
				"myv", "es", "eo", "ext", "eu", "ee", "fa", "hif", "fo", "fr", "fy",
				"ff", "fur", "ga", "gv", "gd", "gl", "gan", "ki", "glk", "gu",
				"got", "hak", "xal", "ko", "ha", "haw", "hy", "hi", "ho", "hsb",
				"hr", "io", "ig", "ilo", "bpy", "id", "ia", "ie", "iu", "ik", "os",
				"xh", "zu", "is", "it", "he", "jv", "kl", "kn", "kr", "pam", "krc",
				"ka", "ks", "csb", "kk", "kw", "rw", "ky", "rn", "sw", "kv", "kg",
				"ht", "ku", "kj", "lad", "lbe", "lo", "la", "lv", "lb", "lt", "lij",
				"li", "ln", "jbo", "lg", "lmo", "hu", "mk", "mg", "ml", "mt", "mi",
				"mr", "arz", "mzn", "ms", "cdo", "mwl", "mdf", "mo", "mn", "mus",
				"my", "nah", "na", "fj", "nl", "nds-nl", "cr", "ne", "new", "ja",
				"nap", "ce", "pih", "no", "nb", "nn", "nrm", "nov", "ii", "oc",
				"mhr", "or", "om", "ng", "hz", "uz", "pa", "pi", "pag", "pnb",
				"pap", "ps", "km", "pcd", "pms", "tpi", "nds", "pl", "tokipona",
				"tp", "pnt", "pt", "aa", "kaa", "crh", "ty", "ksh", "ro", "rmy",
				"rm", "qu", "ru", "sah", "se", "sm", "sa", "sg", "sc", "sco", "stq",
				"st", "tn", "sq", "scn", "si", "simple", "sd", "ss", "sk", "cu",
				"sl", "szl", "so", "ckb", "srn", "sr", "sh", "su", "fi", "sv", "tl",
				"ta", "kab", "roa-tara", "tt", "te", "tet", "th", "ti", "tg", "to",
				"chr", "chy", "ve", "tr", "tk", "tw", "udm", "bug", "uk", "ur",
				"ug", "za", "vec", "vi", "vo", "fiu-vro", "wa", "zh-classical",
				"vls", "war", "wo", "wuu", "ts", "yi", "yo", "zh-yue", "diq", "zea",
				"bat-smg", "zh", "zh-tw", "zh-cn"
			};
            iwikiLinksOrderByLatinFW = new string[] {
				"ace", "af", "ak", "als", "am", "ang", "ab", "ar", "an", "arc",
				"roa-rup", "frp", "arz", "as", "ast", "gn", "av", "ay", "az", "id",
				"ms", "bg", "bm", "zh-min-nan", "nan", "map-bms", "jv", "su", "ba",
				"be", "be-x-old", "bh", "bcl", "bi", "bn", "bo", "bar", "bs", "bpy",
				"br", "bug", "bxr", "ca", "ceb", "ch", "cbk-zam", "sn", "tum", "ny",
				"cho", "chr", "co", "cy", "cv", "cs", "da", "dk", "pdc", "de", "nv",
				"dsb", "na", "dv", "dz", "mh", "et", "el", "eml", "en", "myv", "es",
				"eo", "ext", "eu", "ee", "fa", "hif", "fo", "fr", "fy", "ff", "fur",
				"ga", "gv", "sm", "gd", "gl", "gan", "ki", "glk", "got", "gu", "ha",
				"hak", "xal", "haw", "he", "hi", "ho", "hsb", "hr", "hy", "io",
				"ig", "ii", "ilo", "ia", "ie", "iu", "ik", "os", "xh", "zu", "is",
				"it", "ja", "ka", "kl", "kr", "pam", "krc", "csb", "kk", "kw", "rw",
				"ky", "rn", "sw", "km", "kn", "ko", "kv", "kg", "ht", "ks", "ku",
				"kj", "lad", "lbe", "la", "lv", "to", "lb", "lt", "lij", "li", "ln",
				"lo", "jbo", "lg", "lmo", "hu", "mk", "mg", "mt", "mi", "cdo",
				"mwl", "ml", "mdf", "mo", "mn", "mr", "mus", "my", "mzn", "nah",
				"fj", "ne", "nl", "nds-nl", "cr", "new", "nap", "ce", "pih", "no",
				"nb", "nn", "nrm", "nov", "oc", "mhr", "or", "om", "ng", "hz", "uz",
				"pa", "pag", "pap", "pi", "pcd", "pms", "nds", "pnb", "pl", "pt",
				"pnt", "ps", "aa", "kaa", "crh", "ty", "ksh", "ro", "rmy", "rm",
				"qu", "ru", "sa", "sah", "se", "sg", "sc", "sco", "sd", "stq", "st",
				"tn", "sq", "si", "scn", "simple", "ss", "sk", "sl", "cu", "szl",
				"so", "ckb", "srn", "sr", "sh", "fi", "sv", "ta", "tl", "kab",
				"roa-tara", "tt", "te", "tet", "th", "ti", "vi", "tg", "tokipona",
				"tp", "tpi", "chy", "ve", "tr", "tk", "tw", "udm", "uk", "ur", "ug",
				"za", "vec", "vo", "fiu-vro", "wa", "vls", "war", "wo", "wuu", "ts",
				"yi", "yo", "diq", "zea", "zh", "zh-tw", "zh-cn", "zh-classical",
				"zh-yue", "bat-smg"
			};
            userQueryLists.Add("allpages", "ap"); userQueryLists.Add("alllinks", "al");
            userQueryLists.Add("allusers", "au"); userQueryLists.Add("backlinks", "bl");
            userQueryLists.Add("categorymembers", "cm"); userQueryLists.Add("embeddedin", "ei");
            userQueryLists.Add("imageusage", "iu"); userQueryLists.Add("logevents", "le");
            userQueryLists.Add("recentchanges", "rc"); userQueryLists.Add("usercontribs", "uc");
            userQueryLists.Add("watchlist", "wl"); userQueryLists.Add("exturlusage", "eu");
            userQueryProps.Add("info", "in"); userQueryProps.Add("revisions", "rv");
            userQueryProps.Add("links", "pl"); userQueryProps.Add("langlinks", "ll");
            userQueryProps.Add("images", "im"); userQueryProps.Add("imageinfo", "ii");
            userQueryProps.Add("templates", "tl"); userQueryProps.Add("categories", "cl");
            userQueryProps.Add("extlinks", "el"); userQueryLists.Add("search", "sr");
        }

        /// <summary>Logs in SourceForge.net and retrieves cookies for work with
        /// SourceForge-hosted wikis. That's a special version of LogIn() function.</summary>
        public void LogInSourceForge()
        {
            string postData = string.Format("form_loginname={0}&form_pw={1}" +
                "&ssl_status=&form_rememberme=yes&login=Log+in",
                HttpUtility.UrlEncode(userName.ToLower()), HttpUtility.UrlEncode(userPass));
            string respStr = PostDataAndGetResultHTM("https://sourceforge.net/account/login.php",
                postData, true, false);
            if (respStr.Contains(" class=\"error\""))
                throw new WikiUserException(
                    "\n\n" + User.Msg("Login failed. Check your username and password.") + "\n");
            Console.WriteLine(User.Msg("Logged in SourceForge as {0}."), userName);
        }

        /// <summary>Logs in via api.php and retrieves cookies.</summary>
        public void LogInViaApi()
        {
            string postData = string.Format("lgname={0}&lgpassword={1}&lgdomain={2}",
                HttpUtility.UrlEncode(userName), HttpUtility.UrlEncode(userPass),
                HttpUtility.UrlEncode(userDomain));
            string respStr = PostDataAndGetResultHTM(site + indexPath +
                "api.php?action=login&format=xml", postData, true, false);
            if (respStr.Contains("result=\"Success\""))
            {
                Console.WriteLine(User.Msg("Logged in as {0}."), userName);
                return;
            }

            int tokenPos = respStr.IndexOf("token=\"");
            if (tokenPos < 1)
                throw new WikiUserException(
                    "\n\n" + User.Msg("Login failed. Check your username and password.") + "\n");
            string loginToken = respStr.Substring(tokenPos + 7, 32);
            postData += "&lgtoken=" + HttpUtility.UrlEncode(loginToken);
            respStr = PostDataAndGetResultHTM(site + indexPath +
                "api.php?action=login&format=xml", postData, true, false);
            if (!respStr.Contains("result=\"Success\""))
                throw new WikiUserException(
                    "\n\n" + User.Msg("Login failed. Check your username and password.") + "\n");
            Console.WriteLine(User.Msg("Logged in as {0}."), userName);
        }

        /// <summary>Logs in and retrieves cookies.</summary>
        public void LogIn()
        {
            string loginPageSrc = PostDataAndGetResultHTM(site + indexPath +
                "index.php?title=Special:Userlogin", "", true, true);
            string loginToken = "";
            int loginTokenPos = loginPageSrc.IndexOf(
                "<input type=\"hidden\" name=\"wpLoginToken\" value=\"");
            if (loginTokenPos != -1)
                loginToken = loginPageSrc.Substring(loginTokenPos + 48, 32);

            string postData = string.Format("wpName={0}&wpPassword={1}&wpDomain={2}" +
                "&wpLoginToken={3}&wpRemember=1&wpLoginattempt=Log+in",
                HttpUtility.UrlEncode(userName), HttpUtility.UrlEncode(userPass),
                HttpUtility.UrlEncode(userDomain), HttpUtility.UrlEncode(loginToken));
            string respStr = PostDataAndGetResultHTM(site + indexPath +
                "index.php?title=Special:Userlogin&action=submitlogin&type=login",
                postData, true, false);
            if (respStr.Contains("<div class=\"errorbox\">"))
                throw new WikiUserException(
                    "\n\n" + User.Msg("Login failed. Check your username and password.") + "\n");
            Console.WriteLine(User.Msg("Logged in as {0}."), userName);
        }

        /// <summary>This internal function gets the hypertext markup (HTM) of wiki-page.</summary>
        /// <param name="pageURL">Absolute or relative URL of page to get.</param>
        /// <returns>Returns HTM source code.</returns>
        public string GetPageHTM(string pageURL)
        {
            return PostDataAndGetResultHTM(pageURL, "", false, true);
        }

        /// <summary>This internal function posts specified string to requested resource
        /// and gets the result hypertext markup (HTM).</summary>
        /// <param name="pageURL">Absolute or relative URL of page to get.</param>
        /// <param name="postData">String to post to site with web request.</param>
        /// <returns>Returns code of hypertext markup (HTM).</returns>
        public string PostDataAndGetResultHTM(string pageURL, string postData)
        {
            return PostDataAndGetResultHTM(pageURL, postData, false, true);
        }

        /// <summary>This internal function posts specified string to requested resource
        /// and gets the result hypertext markup (HTM).</summary>
        /// <param name="pageURL">Absolute or relative URL of page to get.</param>
        /// <param name="postData">String to post to site with web request.</param>
        /// <param name="getCookies">If set to true, gets cookies from web response and
        /// saves it in site.cookies container.</param>
        /// <param name="allowRedirect">Allow auto-redirection of web request by server.</param>
        /// <returns>Returns code of hypertext markup (HTM).</returns>
        public string PostDataAndGetResultHTM(string pageURL, string postData, bool getCookies,
            bool allowRedirect)
        {
            if (string.IsNullOrEmpty(pageURL))
                throw new WikiUserException(User.Msg("No URL specified."));
            if (!pageURL.StartsWith(site) && !site.Contains("sourceforge"))
                pageURL = site + pageURL;
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(pageURL);
            webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webReq.UseDefaultCredentials = true;
            webReq.ContentType = Bot.webContentType;
            webReq.UserAgent = Bot.botVer;
            webReq.AllowAutoRedirect = allowRedirect;
            if (cookies.Count == 0)
                webReq.CookieContainer = new CookieContainer();
            else
                webReq.CookieContainer = cookies;
            if (User.unsafeHttpHeaderParsingUsed == 0)
            {
                webReq.ProtocolVersion = HttpVersion.Version10;
                webReq.KeepAlive = false;
            }
            webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            if (!string.IsNullOrEmpty(postData))
            {
                if (User.isRunningOnMono)	// Mono bug 636219 evasion
                    webReq.AllowAutoRedirect = false;
                // https://bugzilla.novell.com/show_bug.cgi?id=636219
                webReq.Method = "POST";
                //webReq.Timeout = 180000;
                byte[] postBytes = Encoding.UTF8.GetBytes(postData);
                webReq.ContentLength = postBytes.Length;
                Stream reqStrm = webReq.GetRequestStream();
                reqStrm.Write(postBytes, 0, postBytes.Length);
                reqStrm.Close();
            }
            HttpWebResponse webResp = null;
            for (int errorCounter = 0; true; errorCounter++)
            {
                try
                {
                    webResp = (HttpWebResponse)webReq.GetResponse();
                    break;
                }
                catch (WebException e)
                {
                    string message = e.Message;
                    if (webReq.AllowAutoRedirect == false &&
                        webResp.StatusCode == HttpStatusCode.Redirect)	// Mono bug 636219 evasion
                        return "";
                    if (Regex.IsMatch(message, ": \\(50[02349]\\) "))
                    {		// Remote problem
                        if (errorCounter > User.retryTimes)
                            throw;
                        Console.Error.WriteLine(message + " " + User.Msg("Retrying in 60 seconds."));
                        Thread.Sleep(60000);
                    }
                    else if (message.Contains("Section=ResponseStatusLine"))
                    {	// Squid problem
                        Bot.SwitchUnsafeHttpHeaderParsing(true);
                        //Console.Write("|");
                        return PostDataAndGetResultHTM(pageURL, postData, getCookies,
                            allowRedirect);
                    }
                    else
                        throw;
                }
            }
            Stream respStream = webResp.GetResponseStream();
            if (webResp.ContentEncoding.ToLower().Contains("gzip"))
                respStream = new GZipStream(respStream, CompressionMode.Decompress);
            else if (webResp.ContentEncoding.ToLower().Contains("deflate"))
                respStream = new DeflateStream(respStream, CompressionMode.Decompress);
            if (getCookies == true)
            {
                Uri siteUri = new Uri(site);
                foreach (Cookie cookie in webResp.Cookies)
                {
                    if (cookie.Domain[0] == '.' &&
                        cookie.Domain.Substring(1) == siteUri.Host)
                        cookie.Domain = cookie.Domain.TrimStart(new char[] { '.' });
                    cookies.Add(cookie);
                }
            }
            StreamReader strmReader = new StreamReader(respStream, Encoding.UTF8);
            string respStr = strmReader.ReadToEnd();
            strmReader.Close();
            webResp.Close();
            return respStr;
        }

        // Another methods added to make it work on WP7

        /// <summary>Opens a text file, reads all lines of the file, and then closes the
        /// file.</summary>
        /// <param name="path">The file(in the Insolated Storage) to open for reading.</param>
        /// <param name="encType">Type encoding used in the file.</param>
        /// <returns>A string array containing all lines of the file.</returns>
        internal string[] ReadAllLines(string path, Encoding encType)
        {
            List<string> tempList = new List<string>();

            try
            {
                using (IsolatedStorageFileStream rawStream = isf.OpenFile("Cache" + System.IO.Path.DirectorySeparatorChar + "Defaults.dat", System.IO.FileMode.Open))
                {
                    StreamReader reader = new StreamReader(rawStream, encType);

                    for (int i = 0; reader.EndOfStream; i++)
                    {
                        tempList[i] = reader.ReadLine();
                    }

                    reader.Close();
                }
            }
            catch
            {
                // TODO: Do something with that Exception.
            }

            return tempList.ToArray();
        }

        /// <summary>Creates a new file, write the contents to the file, and then closes
        /// the file. If the target file already exists, it is overwritten</summary>
        /// <param name="path">The file(in the Insolated Storage) to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        internal void WriteAllText(string path, string contents, Encoding encoding)
        {
            List<string> tempList = new List<string>();

            using (isf)
            {
                if (isf.FileExists(path))
                    isf.DeleteFile(path);

                using (IsolatedStorageFileStream rawStream = isf.CreateFile(path))
                {
                    StreamWriter writer = new StreamWriter(rawStream);

                    //Probably would work with that encoding.
                    writer.WriteLine(contents, encoding);

                    writer.Close();
                }
            }
        }

        /// <summary>A great way of simplifying HTTP GET requests. Developed by Vangos.
        /// Changed by Hazardius 22.06.2012 .
        /// Found on:
        /// http://studentguru.gr/b/vangos/archive/2011/05/17/an-http-post-request-client-for-windows-phone-7.aspx
        /// </summary>
        /// <returns>Downloaded data as a string.</returns>
        internal string HTTPGet()
        {
            string data = null;
            WebClient webClient = new WebClient();
            webClient.UseDefaultCredentials = true;

            webClient.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error == null)
                {
                    data = e.Result;
                }
            };
            webClient.DownloadStringAsync(new Uri(site, UriKind.Absolute));

            return data;
        }
        
        /// <summary>A great way of simplifying HTTP GET requests. Developed by Vangos.
        /// Changed by Hazardius 22.06.2012 .
        /// Found on:
        /// http://studentguru.gr/b/vangos/archive/2011/05/17/an-http-post-request-client-for-windows-phone-7.aspx
        /// Version enabling another sites through absAddress.
        /// </summary>
        /// <param name="absAddress">Absolute URL of requested resource.</param>
        /// <returns>Downloaded data as a string.</returns>
        internal string HTTPGet(string absAddress)
        {
            string data = null;
            WebClient webClient = new WebClient();
            webClient.UseDefaultCredentials = true;

            webClient.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error == null)
                {
                    data = e.Result;
                }
            };
            webClient.DownloadStringAsync(new Uri(absAddress, UriKind.Absolute));

            return data;
        }

        /// <summary>An easy-to-use, thread-safe library which works similarly to
        /// WebClient. Developed by Vangos. Changed by Hazardius 22.06.2012 .
        /// Found on:
        /// http://studentguru.gr/b/vangos/archive/2011/05/17/an-http-post-request-client-for-windows-phone-7.aspx
        /// </summary>
        /// <param name="parameters">Parameters used in POST.</param>
        /// <returns>Downloaded data as a string.</returns>
        internal string HTTPPost(Dictionary<string, object> parameters)
        {
            string data = null;

            WindowsPhonePostClient.PostClient proxy = new WindowsPhonePostClient.PostClient(parameters);
            proxy.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error == null)
                {
                    data = e.Result;
                }
            };
            proxy.DownloadStringAsync(new Uri(site, UriKind.Absolute));

            return data;
        }

        /// <summary>An easy-to-use, thread-safe library which works similarly to
        /// WebClient. Developed by Vangos. Changed by Hazardius 22.06.2012 .
        /// Found on:
        /// http://studentguru.gr/b/vangos/archive/2011/05/17/an-http-post-request-client-for-windows-phone-7.aspx
        /// </summary>
        /// <param name="parameters">Parameters used in POST.</param>
        /// <param name="absAddress">Absolute URL of requested resource.</param>
        /// <returns>Downloaded data as a string.</returns>
        internal string HTTPPost(Dictionary<string, object> parameters, string absAddress)
        {
            string data = null;

            WindowsPhonePostClient.PostClient proxy = new WindowsPhonePostClient.PostClient(parameters);
            proxy.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error == null)
                {
                    data = e.Result;
                }
            };
            proxy.DownloadStringAsync(new Uri(absAddress, UriKind.Absolute));

            return data;
        }
    }
}
