using System.Text.Json;
using System.Text.Json.Nodes;
using IIIF.POC.PostgreSqlRelationalV3Store.Domain;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Services;

public static class ManifestRelationalMapper
{
    private static readonly JsonSerializerOptions CompactJson = new() { WriteIndented = false };
    private static readonly JsonSerializerOptions PrettyJson = new() { WriteIndented = true };

    public static ManifestEntity FromCanonicalJson(string json, string sourceVersion)
    {
        var root = JsonNode.Parse(json)?.AsObject()
            ?? throw new JsonException("The canonical Manifest must be a JSON object.");

        var entity = new ManifestEntity
        {
            IiifId = String(root, "id") ?? throw new JsonException("Manifest id is required."),
            Type = String(root, "type") ?? "Manifest",
            Context = Context(root["@context"]),
            SourceVersion = sourceVersion,
            Rights = String(root, "rights"),
            NavDate = Date(root, "navDate"),
            ViewingDirection = String(root, "viewingDirection"),
            Label = LanguageMap(root["label"]),
            Summary = LanguageMap(root["summary"]),
            Metadata = Objects(root["metadata"]).Select((x, i) => Metadata(x, i)).ToList(),
            RequiredStatement = root["requiredStatement"] is JsonObject required ? RequiredStatement(required) : null,
            Behavior = Strings(root["behavior"]),
            Homepage = Objects(root["homepage"]).Select((x, i) => Link(x, i)).ToList(),
            Thumbnail = Objects(root["thumbnail"]).Select((x, i) => Link(x, i)).ToList(),
            Rendering = Objects(root["rendering"]).Select((x, i) => Link(x, i)).ToList(),
            SeeAlso = Objects(root["seeAlso"]).Select((x, i) => Link(x, i)).ToList(),
            PartOf = Objects(root["partOf"]).Select((x, i) => Link(x, i)).ToList(),
            Provider = Objects(root["provider"]).Select((x, i) => Agent(x, i)).ToList(),
            Services = ServiceObjects(root).Select((x, i) => Service(x, i)).ToList(),
            Items = Objects(root["items"]).Select((x, i) => Canvas(x, i)).ToList(),
            Structures = Objects(root["structures"]).Select((x, i) => Range(x, i)).ToList(),
            Annotations = Objects(root["annotations"]).Select((x, i) => AnnotationPage(x, i)).ToList(),
            PlaceholderCanvas = root["placeholderCanvas"] is JsonObject placeholder ? EmbeddedCanvas(placeholder) : null,
            AccompanyingCanvas = root["accompanyingCanvas"] is JsonObject accompanying ? EmbeddedCanvas(accompanying) : null,
            Start = root["start"] is JsonObject start ? Start(start) : null,
            AdditionalPropertiesJson = Additional(root,
                "@context", "id", "type", "label", "metadata", "summary", "requiredStatement", "rights",
                "navDate", "behavior", "viewingDirection", "provider", "thumbnail", "homepage",
                "rendering", "seeAlso", "service", "services", "partOf", "start", "items", "structures",
                "annotations", "placeholderCanvas", "accompanyingCanvas")
        };

        return entity;
    }

    public static string ToCanonicalJson(ManifestEntity entity, bool indented = false)
    {
        var root = AdditionalObject(entity.AdditionalPropertiesJson);
        root["@context"] = entity.Context;
        root["id"] = entity.IiifId;
        root["type"] = entity.Type;
        PutLanguageMap(root, "label", entity.Label, required: true);
        PutMetadata(root, entity.Metadata);
        PutLanguageMap(root, "summary", entity.Summary);
        if (entity.RequiredStatement is not null)
            root["requiredStatement"] = RequiredStatement(entity.RequiredStatement);
        Put(root, "rights", entity.Rights);
        Put(root, "navDate", entity.NavDate?.ToString("O"));
        PutStrings(root, "behavior", entity.Behavior);
        Put(root, "viewingDirection", entity.ViewingDirection);
        PutObjects(root, "provider", entity.Provider, Agent);
        PutObjects(root, "thumbnail", entity.Thumbnail, Link);
        PutObjects(root, "homepage", entity.Homepage, Link);
        PutObjects(root, "rendering", entity.Rendering, Link);
        PutObjects(root, "seeAlso", entity.SeeAlso, Link);
        PutObjects(root, "services", entity.Services, Service);
        PutObjects(root, "partOf", entity.PartOf, Link);
        if (entity.Start is not null) root["start"] = Start(entity.Start);
        PutObjects(root, "items", entity.Items, Canvas, required: true);
        PutObjects(root, "structures", entity.Structures, Range);
        PutObjects(root, "annotations", entity.Annotations, AnnotationPage);
        if (entity.PlaceholderCanvas is not null) root["placeholderCanvas"] = EmbeddedCanvas(entity.PlaceholderCanvas);
        if (entity.AccompanyingCanvas is not null) root["accompanyingCanvas"] = EmbeddedCanvas(entity.AccompanyingCanvas);
        return root.ToJsonString(indented ? PrettyJson : CompactJson);
    }

    public static string FirstLabel(LanguageMapEntity map) =>
        map.Values.OrderBy(x => x.Position).Select(x => x.Value).FirstOrDefault() ?? "Untitled Manifest";

    private static CanvasEntity Canvas(JsonObject obj, int position) => new()
    {
        Position = position,
        IiifId = String(obj, "id") ?? throw new JsonException("Canvas id is required."),
        Type = String(obj, "type") ?? "Canvas",
        Height = Int(obj, "height"),
        Width = Int(obj, "width"),
        Duration = Double(obj, "duration"),
        Rights = String(obj, "rights"),
        NavDate = Date(obj, "navDate"),
        ViewingDirection = String(obj, "viewingDirection"),
        Label = LanguageMap(obj["label"]),
        Summary = LanguageMap(obj["summary"]),
        Metadata = Objects(obj["metadata"]).Select((x, i) => Metadata(x, i)).ToList(),
        RequiredStatement = obj["requiredStatement"] is JsonObject required ? RequiredStatement(required) : null,
        Behavior = Strings(obj["behavior"]),
        Homepage = Objects(obj["homepage"]).Select((x, i) => Link(x, i)).ToList(),
        Thumbnail = Objects(obj["thumbnail"]).Select((x, i) => Link(x, i)).ToList(),
        Rendering = Objects(obj["rendering"]).Select((x, i) => Link(x, i)).ToList(),
        SeeAlso = Objects(obj["seeAlso"]).Select((x, i) => Link(x, i)).ToList(),
        PartOf = Objects(obj["partOf"]).Select((x, i) => Link(x, i)).ToList(),
        Provider = Objects(obj["provider"]).Select((x, i) => Agent(x, i)).ToList(),
        Services = ServiceObjects(obj).Select((x, i) => Service(x, i)).ToList(),
        Items = Objects(obj["items"]).Select((x, i) => AnnotationPage(x, i)).ToList(),
        Annotations = Objects(obj["annotations"]).Select((x, i) => AnnotationPage(x, i)).ToList(),
        PlaceholderCanvas = obj["placeholderCanvas"] is JsonObject placeholder ? EmbeddedCanvas(placeholder) : null,
        AccompanyingCanvas = obj["accompanyingCanvas"] is JsonObject accompanying ? EmbeddedCanvas(accompanying) : null,
        AdditionalPropertiesJson = Additional(obj,
            "id", "type", "label", "metadata", "summary", "requiredStatement", "rights", "navDate",
            "language", "behavior", "viewingDirection", "provider", "thumbnail", "homepage", "rendering",
            "seeAlso", "service", "services", "partOf", "items", "annotations", "placeholderCanvas",
            "accompanyingCanvas", "height", "width", "duration")
    };

    private static EmbeddedCanvasEntity EmbeddedCanvas(JsonObject obj) => new()
    {
        IiifId = String(obj, "id") ?? throw new JsonException("Embedded Canvas id is required."),
        Type = String(obj, "type") ?? "Canvas",
        Height = Int(obj, "height"),
        Width = Int(obj, "width"),
        Duration = Double(obj, "duration"),
        Label = LanguageMap(obj["label"]),
        Items = Objects(obj["items"]).Select((x, i) => AnnotationPage(x, i)).ToList(),
        AdditionalPropertiesJson = Additional(obj, "id", "type", "label", "items", "height", "width", "duration")
    };

    private static AnnotationPageEntity AnnotationPage(JsonObject obj, int position, bool allowBodyResourceAnnotations = true) => new()
    {
        Position = position,
        IiifId = String(obj, "id") ?? throw new JsonException("AnnotationPage id is required."),
        Type = String(obj, "type") ?? "AnnotationPage",
        Items = Objects(obj["items"]).Select((x, i) => Annotation(x, i, allowBodyResourceAnnotations)).ToList(),
        AdditionalPropertiesJson = Additional(obj, "id", "type", "items")
    };

    private static AnnotationEntity Annotation(JsonObject obj, int position, bool allowBodyResourceAnnotations) => new()
    {
        Position = position,
        IiifId = String(obj, "id") ?? throw new JsonException("Annotation id is required."),
        Type = String(obj, "type") ?? "Annotation",
        Label = LanguageMap(obj["label"]),
        Motivation = Strings(obj["motivation"]),
        Bodies = BodyNodes(obj["body"]).Select((x, i) => ContentResource(x, i, allowBodyResourceAnnotations)).ToList(),
        Targets = TargetNodes(obj["target"]).Select((x, i) => Target(x, i)).ToList(),
        TimeMode = String(obj, "timeMode"),
        AdditionalPropertiesJson = Additional(obj, "id", "type", "label", "motivation", "body", "target", "timeMode")
    };

    private static ContentResourceEntity ContentResource(JsonNode node, int position, bool allowResourceAnnotations)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var id))
            return new ContentResourceEntity { Position = position, Id = id, Type = "Text" };

        var obj = node.AsObject();
        var type = String(obj, "type") ?? "Image";
        var entity = new ContentResourceEntity
        {
            Position = position,
            Id = String(obj, "id"),
            Type = type,
            Format = String(obj, "format"),
            Profile = String(obj, "profile"),
            Height = Int(obj, "height"),
            Width = Int(obj, "width"),
            Duration = Double(obj, "duration"),
            Value = String(obj, "value"),
            TextDirection = String(obj, "direction"),
            Label = LanguageMap(obj["label"]),
            Language = Strings(obj["language"]),
            Behavior = Strings(obj["behavior"]),
            Services = ServiceObjects(obj).Select((x, i) => Service(x, i)).ToList(),
            Annotations = ParseResourceAnnotations(obj["annotations"], allowResourceAnnotations),
            Items = type == "Choice" ? Objects(obj["items"]).Select((x, i) => ChoiceItem(x, i)).ToList() : [],
            SpecificResource = type == "SpecificResource" ? SpecificResource(obj) : null,
            AdditionalPropertiesJson = Additional(obj,
                "id", "type", "format", "profile", "height", "width", "duration", "value", "direction",
                "label", "language", "behavior", "service", "services", "annotations", "items", "source",
                "selector", "styleClass")
        };
        return entity;
    }

    private static List<AnnotationPageEntity> ParseResourceAnnotations(JsonNode? node, bool allowResourceAnnotations)
    {
        var pages = Objects(node).ToList();
        if (pages.Count == 0) return [];
        if (!allowResourceAnnotations)
            throw new NotSupportedException("This POC supports one relational level of annotations on content resources. Deeper recursive resource annotations are rejected rather than stored as JSONB.");
        return pages.Select((x, i) => AnnotationPage(x, i, false)).ToList();
    }

    private static ChoiceItemEntity ChoiceItem(JsonObject obj, int position) => new()
    {
        Position = position,
        Id = String(obj, "id"),
        Type = String(obj, "type") ?? "Image",
        Format = String(obj, "format"),
        Profile = String(obj, "profile"),
        Height = Int(obj, "height"),
        Width = Int(obj, "width"),
        Duration = Double(obj, "duration"),
        Value = String(obj, "value"),
        Label = LanguageMap(obj["label"]),
        Language = Strings(obj["language"]),
        Services = ServiceObjects(obj).Select((x, i) => Service(x, i)).ToList(),
        AdditionalPropertiesJson = Additional(obj,
            "id", "type", "format", "profile", "height", "width", "duration", "value", "label",
            "language", "service", "services")
    };

    private static SpecificResourceEntity SpecificResource(JsonObject obj)
    {
        var source = obj["source"];
        return new SpecificResourceEntity
        {
            Type = String(obj, "type") ?? "SpecificResource",
            SourceId = ReferenceId(source),
            SourceType = ReferenceType(source),
            Selector = obj["selector"] is JsonObject selector ? Selector(selector) : null,
            StyleClass = String(obj, "styleClass"),
            AdditionalPropertiesJson = Additional(obj, "id", "type", "source", "selector", "styleClass")
        };
    }

    private static TargetEntity Target(JsonNode node, int position)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var id))
            return new TargetEntity { Position = position, Id = id };

        var obj = node.AsObject();
        var source = obj["source"];
        return new TargetEntity
        {
            Position = position,
            Id = String(obj, "id"),
            Type = String(obj, "type"),
            SourceId = ReferenceId(source),
            SourceType = ReferenceType(source),
            Selector = obj["selector"] is JsonObject selector ? Selector(selector) : null,
            AdditionalPropertiesJson = Additional(obj, "id", "type", "source", "selector")
        };
    }

    private static SelectorEntity Selector(JsonObject obj) => new()
    {
        Id = String(obj, "id"),
        Type = String(obj, "type") ?? "FragmentSelector",
        Value = String(obj, "value"),
        ConformsTo = String(obj, "conformsTo"),
        X = Int(obj, "x"),
        Y = Int(obj, "y"),
        T = Int(obj, "t"),
        AdditionalPropertiesJson = Additional(obj, "id", "type", "value", "conformsTo", "x", "y", "t")
    };

    private static RangeEntity Range(JsonObject obj, int position) => new()
    {
        Position = position,
        IiifId = String(obj, "id") ?? throw new JsonException("Range id is required."),
        Type = String(obj, "type") ?? "Range",
        Rights = String(obj, "rights"),
        NavDate = Date(obj, "navDate"),
        ViewingDirection = String(obj, "viewingDirection"),
        Label = LanguageMap(obj["label"]),
        Summary = LanguageMap(obj["summary"]),
        Metadata = Objects(obj["metadata"]).Select((x, i) => Metadata(x, i)).ToList(),
        RequiredStatement = obj["requiredStatement"] is JsonObject required ? RequiredStatement(required) : null,
        Behavior = Strings(obj["behavior"]),
        Homepage = Objects(obj["homepage"]).Select((x, i) => Link(x, i)).ToList(),
        Thumbnail = Objects(obj["thumbnail"]).Select((x, i) => Link(x, i)).ToList(),
        Rendering = Objects(obj["rendering"]).Select((x, i) => Link(x, i)).ToList(),
        SeeAlso = Objects(obj["seeAlso"]).Select((x, i) => Link(x, i)).ToList(),
        PartOf = Objects(obj["partOf"]).Select((x, i) => Link(x, i)).ToList(),
        Provider = Objects(obj["provider"]).Select((x, i) => Agent(x, i)).ToList(),
        Services = ServiceObjects(obj).Select((x, i) => Service(x, i)).ToList(),
        Items = Objects(obj["items"]).Select((x, i) => RangeItem(x, i)).ToList(),
        Annotations = Objects(obj["annotations"]).Select((x, i) => AnnotationPage(x, i)).ToList(),
        Supplementary = obj["supplementary"] is JsonObject supplementary ? Link(supplementary, 0) : null,
        AdditionalPropertiesJson = Additional(obj,
            "id", "type", "label", "metadata", "summary", "requiredStatement", "rights", "navDate",
            "language", "behavior", "viewingDirection", "provider", "thumbnail", "homepage", "rendering",
            "seeAlso", "service", "services", "partOf", "items", "annotations", "supplementary")
    };

    private static RangeItemEntity RangeItem(JsonObject obj, int position)
    {
        var source = obj["source"];
        return new RangeItemEntity
        {
            Position = position,
            Id = String(obj, "id"),
            Type = String(obj, "type") ?? "Canvas",
            SourceId = ReferenceId(source),
            SourceType = ReferenceType(source),
            Selector = obj["selector"] is JsonObject selector ? Selector(selector) : null,
            Label = LanguageMap(obj["label"]),
            AdditionalPropertiesJson = Additional(obj, "id", "type", "source", "selector", "label")
        };
    }

    private static StartEntity Start(JsonObject obj)
    {
        var source = obj["source"];
        return new StartEntity
        {
            Id = String(obj, "id"),
            Type = String(obj, "type") ?? "Canvas",
            SourceId = ReferenceId(source),
            SourceType = ReferenceType(source),
            Selector = obj["selector"] is JsonObject selector ? Selector(selector) : null,
            AdditionalPropertiesJson = Additional(obj, "id", "type", "source", "selector")
        };
    }

    private static MetadataEntity Metadata(JsonObject obj, int position) => new()
    {
        Position = position,
        Label = LanguageMap(obj["label"]),
        Value = LanguageMap(obj["value"])
    };

    private static RequiredStatementEntity RequiredStatement(JsonObject obj) => new()
    {
        Label = LanguageMap(obj["label"]),
        Value = LanguageMap(obj["value"])
    };

    private static LinkEntity Link(JsonObject obj, int position) => new()
    {
        Position = position,
        Id = String(obj, "id"),
        Type = String(obj, "type"),
        Format = String(obj, "format"),
        Profile = String(obj, "profile"),
        Height = Int(obj, "height"),
        Width = Int(obj, "width"),
        Duration = Double(obj, "duration"),
        Label = LanguageMap(obj["label"]),
        Language = Strings(obj["language"]),
        Services = ServiceObjects(obj).Select((x, i) => Service(x, i)).ToList(),
        AdditionalPropertiesJson = Additional(obj,
            "id", "type", "format", "profile", "height", "width", "duration", "label", "language",
            "service", "services")
    };

    private static ServiceEntity Service(JsonObject obj, int position) => new()
    {
        Position = position,
        Context = Context(obj["@context"]),
        Id = String(obj, "id") ?? String(obj, "@id"),
        Type = String(obj, "type") ?? String(obj, "@type"),
        Profile = String(obj, "profile"),
        AdditionalPropertiesJson = Additional(obj, "@context", "id", "@id", "type", "@type", "profile")
    };

    private static AgentEntity Agent(JsonObject obj, int position) => new()
    {
        Position = position,
        Id = String(obj, "id"),
        Type = String(obj, "type") ?? "Agent",
        Label = LanguageMap(obj["label"]),
        Homepage = Objects(obj["homepage"]).Select((x, i) => Link(x, i)).ToList(),
        Logo = Objects(obj["logo"]).Select((x, i) => Link(x, i)).ToList(),
        SeeAlso = Objects(obj["seeAlso"]).Select((x, i) => Link(x, i)).ToList(),
        Services = ServiceObjects(obj).Select((x, i) => Service(x, i)).ToList(),
        AdditionalPropertiesJson = Additional(obj, "id", "type", "label", "homepage", "logo", "seeAlso", "service", "services")
    };

    private static LanguageMapEntity LanguageMap(JsonNode? node)
    {
        var map = new LanguageMapEntity();
        if (node is not JsonObject obj) return map;
        var position = 0;
        foreach (var property in obj)
        {
            foreach (var value in StringsRaw(property.Value))
            {
                map.Values.Add(new LanguageValueEntity
                {
                    Position = position++,
                    Language = property.Key,
                    Value = value
                });
            }
        }
        return map;
    }

    private static List<OrderedStringEntity> Strings(JsonNode? node) =>
        StringsRaw(node).Select((x, i) => new OrderedStringEntity { Position = i, Value = x }).ToList();

    private static IEnumerable<string> StringsRaw(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var single))
        {
            yield return single;
            yield break;
        }
        if (node is not JsonArray array) yield break;
        foreach (var item in array)
            if (item is JsonValue text && text.TryGetValue<string>(out var result))
                yield return result;
    }

    private static IEnumerable<JsonObject> Objects(JsonNode? node)
    {
        if (node is JsonObject single)
        {
            yield return single;
            yield break;
        }
        if (node is not JsonArray array) yield break;
        foreach (var item in array)
            if (item is JsonObject obj)
                yield return obj;
    }

    private static IEnumerable<JsonObject> ServiceObjects(JsonObject obj) =>
        Objects(obj["service"]).Concat(Objects(obj["services"]));

    private static IEnumerable<JsonNode> BodyNodes(JsonNode? node) => Nodes(node);
    private static IEnumerable<JsonNode> TargetNodes(JsonNode? node) => Nodes(node);

    private static IEnumerable<JsonNode> Nodes(JsonNode? node)
    {
        if (node is null) yield break;
        if (node is JsonArray array)
        {
            foreach (var item in array)
                if (item is not null) yield return item;
            yield break;
        }
        yield return node;
    }

    private static string Additional(JsonObject obj, params string[] known)
    {
        var knownSet = known.ToHashSet(StringComparer.Ordinal);
        var additional = new JsonObject();
        foreach (var property in obj)
            if (!knownSet.Contains(property.Key))
                additional[property.Key] = property.Value?.DeepClone();
        return additional.ToJsonString(CompactJson);
    }

    private static JsonObject AdditionalObject(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new JsonObject();
        try { return JsonNode.Parse(json)?.AsObject() ?? new JsonObject(); }
        catch (JsonException) { return new JsonObject(); }
    }

    private static string? String(JsonObject obj, string name) =>
        obj[name] is JsonValue value && value.TryGetValue<string>(out var result) ? result : null;

    private static int? Int(JsonObject obj, string name) =>
        obj[name] is JsonValue value && value.TryGetValue<int>(out var result) ? result : null;

    private static double? Double(JsonObject obj, string name) =>
        obj[name] is JsonValue value && value.TryGetValue<double>(out var result) ? result : null;

    private static DateTimeOffset? Date(JsonObject obj, string name) =>
        DateTimeOffset.TryParse(String(obj, name), out var value) ? value : null;

    private static string Context(JsonNode? node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var result)) return result;
        if (node is JsonArray array)
            return array.Select(x => x?.GetValue<string>()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                   ?? "http://iiif.io/api/presentation/3/context.json";
        return "http://iiif.io/api/presentation/3/context.json";
    }

    private static string? ReferenceId(JsonNode? node) => node switch
    {
        JsonValue value when value.TryGetValue<string>(out var id) => id,
        JsonObject obj => String(obj, "id"),
        _ => null
    };

    private static string? ReferenceType(JsonNode? node) => node is JsonObject obj ? String(obj, "type") : null;

    private static JsonObject Canvas(CanvasEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        obj["id"] = entity.IiifId; obj["type"] = entity.Type;
        PutLanguageMap(obj, "label", entity.Label);
        PutMetadata(obj, entity.Metadata); PutLanguageMap(obj, "summary", entity.Summary);
        if (entity.RequiredStatement is not null) obj["requiredStatement"] = RequiredStatement(entity.RequiredStatement);
        Put(obj, "rights", entity.Rights); Put(obj, "navDate", entity.NavDate?.ToString("O"));
        PutStrings(obj, "behavior", entity.Behavior); Put(obj, "viewingDirection", entity.ViewingDirection);
        PutObjects(obj, "provider", entity.Provider, Agent); PutObjects(obj, "thumbnail", entity.Thumbnail, Link);
        PutObjects(obj, "homepage", entity.Homepage, Link); PutObjects(obj, "rendering", entity.Rendering, Link);
        PutObjects(obj, "seeAlso", entity.SeeAlso, Link); PutObjects(obj, "service", entity.Services, Service);
        PutObjects(obj, "partOf", entity.PartOf, Link); Put(obj, "height", entity.Height); Put(obj, "width", entity.Width);
        Put(obj, "duration", entity.Duration); PutObjects(obj, "items", entity.Items, AnnotationPage);
        PutObjects(obj, "annotations", entity.Annotations, AnnotationPage);
        if (entity.PlaceholderCanvas is not null) obj["placeholderCanvas"] = EmbeddedCanvas(entity.PlaceholderCanvas);
        if (entity.AccompanyingCanvas is not null) obj["accompanyingCanvas"] = EmbeddedCanvas(entity.AccompanyingCanvas);
        return obj;
    }

    private static JsonObject EmbeddedCanvas(EmbeddedCanvasEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        obj["id"] = entity.IiifId; obj["type"] = entity.Type;
        PutLanguageMap(obj, "label", entity.Label); Put(obj, "height", entity.Height); Put(obj, "width", entity.Width);
        Put(obj, "duration", entity.Duration); PutObjects(obj, "items", entity.Items, AnnotationPage);
        return obj;
    }

    private static JsonObject AnnotationPage(AnnotationPageEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        obj["id"] = entity.IiifId; obj["type"] = entity.Type;
        PutObjects(obj, "items", entity.Items, Annotation, required: true);
        return obj;
    }

    private static JsonObject Annotation(AnnotationEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        obj["id"] = entity.IiifId; obj["type"] = entity.Type;
        PutLanguageMap(obj, "label", entity.Label); PutStrings(obj, "motivation", entity.Motivation, singleWhenOne: true);
        PutBodyOrTarget(obj, "body", entity.Bodies.OrderBy(x => x.Position).Select(ContentResource).ToList());
        PutBodyOrTarget(obj, "target", entity.Targets.OrderBy(x => x.Position).Select(Target).ToList());
        Put(obj, "timeMode", entity.TimeMode);
        return obj;
    }

    private static JsonNode ContentResource(ContentResourceEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        Put(obj, "id", entity.Id); obj["type"] = entity.Type; Put(obj, "format", entity.Format); Put(obj, "profile", entity.Profile);
        Put(obj, "height", entity.Height); Put(obj, "width", entity.Width); Put(obj, "duration", entity.Duration);
        Put(obj, "value", entity.Value); Put(obj, "direction", entity.TextDirection); PutLanguageMap(obj, "label", entity.Label);
        PutStrings(obj, "language", entity.Language, singleWhenOne: true); PutStrings(obj, "behavior", entity.Behavior);
        PutObjects(obj, "service", entity.Services, Service); PutObjects(obj, "annotations", entity.Annotations, AnnotationPage);
        if (entity.Type == "Choice") PutObjects(obj, "items", entity.Items, ChoiceItem, required: true);
        if (entity.Type == "SpecificResource" && entity.SpecificResource is not null)
        {
            PutSource(obj, entity.SpecificResource.SourceId, entity.SpecificResource.SourceType);
            if (entity.SpecificResource.Selector is not null) obj["selector"] = Selector(entity.SpecificResource.Selector);
            Put(obj, "styleClass", entity.SpecificResource.StyleClass);
            MergeAdditional(obj, entity.SpecificResource.AdditionalPropertiesJson);
        }
        return obj;
    }

    private static JsonObject ChoiceItem(ChoiceItemEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        Put(obj, "id", entity.Id); obj["type"] = entity.Type; Put(obj, "format", entity.Format); Put(obj, "profile", entity.Profile);
        Put(obj, "height", entity.Height); Put(obj, "width", entity.Width); Put(obj, "duration", entity.Duration);
        Put(obj, "value", entity.Value); PutLanguageMap(obj, "label", entity.Label);
        PutStrings(obj, "language", entity.Language, singleWhenOne: true); PutObjects(obj, "service", entity.Services, Service);
        return obj;
    }

    private static JsonNode Target(TargetEntity entity)
    {
        if (!string.IsNullOrWhiteSpace(entity.Id) && entity.SourceId is null && entity.Selector is null && entity.Type is null && entity.AdditionalPropertiesJson == "{}")
            return JsonValue.Create(entity.Id)!;
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        Put(obj, "id", entity.Id); Put(obj, "type", entity.Type);
        PutSource(obj, entity.SourceId, entity.SourceType);
        if (entity.Selector is not null) obj["selector"] = Selector(entity.Selector);
        return obj;
    }

    private static JsonObject Selector(SelectorEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        Put(obj, "id", entity.Id); obj["type"] = entity.Type; Put(obj, "value", entity.Value);
        Put(obj, "conformsTo", entity.ConformsTo); Put(obj, "x", entity.X); Put(obj, "y", entity.Y); Put(obj, "t", entity.T);
        return obj;
    }

    private static JsonObject Range(RangeEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        obj["id"] = entity.IiifId; obj["type"] = entity.Type; PutLanguageMap(obj, "label", entity.Label, required: true);
        PutMetadata(obj, entity.Metadata); PutLanguageMap(obj, "summary", entity.Summary);
        if (entity.RequiredStatement is not null) obj["requiredStatement"] = RequiredStatement(entity.RequiredStatement);
        Put(obj, "rights", entity.Rights); Put(obj, "navDate", entity.NavDate?.ToString("O"));
        PutStrings(obj, "behavior", entity.Behavior); Put(obj, "viewingDirection", entity.ViewingDirection);
        PutObjects(obj, "provider", entity.Provider, Agent); PutObjects(obj, "thumbnail", entity.Thumbnail, Link);
        PutObjects(obj, "homepage", entity.Homepage, Link); PutObjects(obj, "rendering", entity.Rendering, Link);
        PutObjects(obj, "seeAlso", entity.SeeAlso, Link); PutObjects(obj, "service", entity.Services, Service);
        PutObjects(obj, "partOf", entity.PartOf, Link); PutObjects(obj, "items", entity.Items, RangeItem);
        PutObjects(obj, "annotations", entity.Annotations, AnnotationPage);
        if (entity.Supplementary is not null) obj["supplementary"] = Link(entity.Supplementary);
        return obj;
    }

    private static JsonObject RangeItem(RangeItemEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        Put(obj, "id", entity.Id); obj["type"] = entity.Type; PutLanguageMap(obj, "label", entity.Label);
        PutSource(obj, entity.SourceId, entity.SourceType);
        if (entity.Selector is not null) obj["selector"] = Selector(entity.Selector);
        return obj;
    }

    private static JsonObject Start(StartEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        Put(obj, "id", entity.Id); obj["type"] = entity.Type; PutSource(obj, entity.SourceId, entity.SourceType);
        if (entity.Selector is not null) obj["selector"] = Selector(entity.Selector);
        return obj;
    }

    private static JsonObject Link(LinkEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        Put(obj, "id", entity.Id); Put(obj, "type", entity.Type); Put(obj, "format", entity.Format);
        Put(obj, "profile", entity.Profile); Put(obj, "height", entity.Height); Put(obj, "width", entity.Width);
        Put(obj, "duration", entity.Duration); PutLanguageMap(obj, "label", entity.Label);
        PutStrings(obj, "language", entity.Language, singleWhenOne: true); PutObjects(obj, "service", entity.Services, Service);
        return obj;
    }

    private static JsonObject Service(ServiceEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        Put(obj, "@context", entity.Context); Put(obj, "id", entity.Id); Put(obj, "type", entity.Type); Put(obj, "profile", entity.Profile);
        return obj;
    }

    private static JsonObject Agent(AgentEntity entity)
    {
        var obj = AdditionalObject(entity.AdditionalPropertiesJson);
        Put(obj, "id", entity.Id); obj["type"] = entity.Type; PutLanguageMap(obj, "label", entity.Label, required: true);
        PutObjects(obj, "homepage", entity.Homepage, Link); PutObjects(obj, "logo", entity.Logo, Link);
        PutObjects(obj, "seeAlso", entity.SeeAlso, Link); PutObjects(obj, "service", entity.Services, Service);
        return obj;
    }

    private static JsonObject RequiredStatement(RequiredStatementEntity entity)
    {
        var obj = new JsonObject();
        PutLanguageMap(obj, "label", entity.Label, required: true); PutLanguageMap(obj, "value", entity.Value, required: true);
        return obj;
    }

    private static void PutMetadata(JsonObject obj, IEnumerable<MetadataEntity> metadata)
    {
        var values = metadata.OrderBy(x => x.Position).Select(x =>
        {
            var item = new JsonObject();
            PutLanguageMap(item, "label", x.Label, required: true); PutLanguageMap(item, "value", x.Value, required: true);
            return (JsonNode)item;
        }).ToArray();
        if (values.Length > 0) obj["metadata"] = new JsonArray(values);
    }

    private static void PutLanguageMap(JsonObject obj, string property, LanguageMapEntity map, bool required = false)
    {
        var grouped = map.Values.OrderBy(x => x.Position).GroupBy(x => x.Language);
        var result = new JsonObject();
        foreach (var group in grouped)
            result[group.Key] = new JsonArray(group.Select(x => JsonValue.Create(x.Value)).ToArray());
        if (required || result.Count > 0) obj[property] = result;
    }

    private static void PutStrings(JsonObject obj, string property, IEnumerable<OrderedStringEntity> values, bool singleWhenOne = false)
    {
        var ordered = values.OrderBy(x => x.Position).Select(x => x.Value).ToArray();
        if (ordered.Length == 0) return;
        obj[property] = singleWhenOne && ordered.Length == 1
            ? JsonValue.Create(ordered[0])
            : new JsonArray(ordered.Select(x => JsonValue.Create(x)).ToArray());
    }

    private static void PutObjects<T>(JsonObject obj, string property, IEnumerable<T> values, Func<T, JsonObject> mapper, bool required = false)
    {
        var array = new JsonArray(values.OrderBy(x => PositionOf(x)).Select(x => (JsonNode)mapper(x)).ToArray());
        if (required || array.Count > 0) obj[property] = array;
    }

    private static int PositionOf<T>(T value) => value switch
    {
        CanvasEntity x => x.Position, RangeEntity x => x.Position, AnnotationPageEntity x => x.Position,
        AnnotationEntity x => x.Position, ContentResourceEntity x => x.Position, ChoiceItemEntity x => x.Position,
        TargetEntity x => x.Position, RangeItemEntity x => x.Position, MetadataEntity x => x.Position,
        LinkEntity x => x.Position, ServiceEntity x => x.Position, AgentEntity x => x.Position,
        _ => 0
    };

    private static void PutBodyOrTarget(JsonObject obj, string property, IReadOnlyList<JsonNode> values)
    {
        if (values.Count == 0) return;
        obj[property] = values.Count == 1 ? values[0] : new JsonArray(values.ToArray());
    }

    private static void PutSource(JsonObject obj, string? id, string? type)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        if (string.IsNullOrWhiteSpace(type)) obj["source"] = id;
        else obj["source"] = new JsonObject { ["id"] = id, ["type"] = type };
    }

    private static void Put(JsonObject obj, string property, object? value)
    {
        if (value is null) return;
        obj[property] = JsonSerializer.SerializeToNode(value);
    }

    private static void MergeAdditional(JsonObject target, string json)
    {
        foreach (var property in AdditionalObject(json))
            if (!target.ContainsKey(property.Key))
                target[property.Key] = property.Value?.DeepClone();
    }
}
