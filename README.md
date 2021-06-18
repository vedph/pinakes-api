﻿# Pinakes API

An API wrapping an essential search engine for the RAP subset of the Pinakes database.

## Pinakes Database

This section empirically describes a subset of the Pinakes DB, as illustrated by this schema:

![schema.png](schema.png)

### Keyword

A keyword attached to any entity (`keywords`). They may have a type (`keywords_types`).

For our purposes, we have keywords attached to authors, works, and recensions.

### Work

- `oeuvres`: essentially title and date.
- `oeuvres_alias`: alternative titles for this work.
- `oeuvres_auteurs`: the authors of this work (from `auteurs`). Each connection to an author specifies his role (from `roles`). There is also a `principal` numeric value.
- `identifiants_oeuvres`: the IDs attached to this work (from `identifiants`).
- `keywords_oeuvres`: the keywords attached to this work (from `keywords`).

A work can have a number of related works via:

- `oeuvres_attributs`: a direct connection to another work, eventually with a free text comment.
- `relations`: supposedly, these connect two _works_ (even though there is no explicit formalism in the schema, as no foreign key is explicitly set as a constraint in this table). They seem to be a later addition covering a more structured type of relation, where besides a comment we also have a specific relation type, derived from a dedicated table (`relations_types`). This is more fit to a LOD-like scenario, as it's easy to look at these relations between a parent and a child as triples, where two entities are connected via a predicate, which in turn is another entity. All these relations are mutual, so that for each relation there is a parent and a child role specified (e.g. has-part vs. is-part-of).

### Author

- `auteurs`: essentially name and century and date (both are strings with various expressions). A flag tells if the author is rather a category of works (e.g. aenigmata).
- `auteurs_alias`: alternative names for this author.
- `identifiants_auteurs`: the IDs attached to this author (from `identifiants`).
- `keywords_auteurs`: the keywords attached to this author (from `keywords`).

### Recensions

- `recensions`: essentially recension, title, a number, century and date, and a place.
- `identifiants_recensions`: the IDs attached to this recensio (from `identifiants`).
- `keywords_recensions`: the keywords attached to this recensio (from `keywords`).
- `recensions_auteurs`: the authors of this work (from `auteurs`). Each connection to an author specifies his role (from `roles`).

### Manuscripts

As for the relationship with works, here the atom is represented by the codicological unit. This belongs to some manuscript. A manuscript has 1 or more codicological units, and each codicological unit has 1 or more witnesses. So the conceptual relationship is manuscript - codicological unit - witness - [recensio] - work.

Every unit has 0 or more witnesses, which are the sources for works, or recensions. Recensions are used only when a work can be considered to have several states of its text which are considerably different. In most cases, there is just a single work with no recension, and this work is directly connected to the witnesses. When instead we have several recensions, then the witnesses are parted among them: recension A has witnesses X and Y, recension B has witness Z, etc.

- `temoins`: works (or recensions) have 0 or more witnesses (`temoins`). A witness essentially has sheets range, century, date (more granular, with min and max), a title, incipit/desinit, and corresponds to a codicological unit. The witness is the intersection between object and text. There is a single date per witness corresponding to that of the codicological unit, if specified; so the date to be considered here is the witness' date.

- `unites_codicologiques`: each witness corresponds to a codicological unit via its `id_uc` field, which is the FK for codicological units, `unites_codicologiques`.`id`. A codicological unit too has title, sheet range, century and date with min and max, genre, place, and a parent manuscript.

- `cotes`: each codicological unit belongs to a manuscript via its `id_cote`, which is the FK for `cotes`.`id`. A manuscript essentially has title and type, belongs to a fund, and is connected to physical description via `pui_ptb`.

## Search

Note: the sample SQL has the only purpose of letting reader visualize some data. Real queries will be composed via CTEs or subqueries.

All the searches on text fields (those marked with `[T]` below) are subject to this processing:

1. the text is filtered to remove noise (e.g. typically whitespaces are normalized, only letters, digits, apostrophes are preserved, diacritics are removed, letters are lowercased).
2. the filtered text is tokenized, i.e. split into "words".
3. the tokens (words) are further filtered (e.g. to remove stopwords, generate transliterated forms, etc.).
4. tokens are stored in an index, each with all its occurrences.

This way you can look for any word or part of a word in any of the indexed fields. This words-index can then be searched in combination with other metadata like dates, types, etc. as specified below in the filters for each search target.

### Text Pipeline

The details for text filtering and tokenization pipeline are as follows:

- HTML/XML tags are removed.
- separators are preprocessed (this is useful only when we have corner cases where e.g. several variants of a name are entered in a field with a separator like slash, e.g. `Rome/Roma`. In this case, the `/` must be treated as a token separator, otherwise it would just be ignored and `RomeRoma` would be the resulting "word").
- whitespaces are normalized.
- diacritics are removed.
- case differences are flattened.
- tokenization uses whitespaces and apostrophe (`'`) only as separator. This is usually enough for DB text fields: e.g. in English texts we treat `'` as a separator to split forms like `it's` into `it` and `s`.
- a list of stopwords (e.g. "the", "a", "of"...) from French, English, Italian, and Greek will be provided to avoid indexing words which can be treated as irrelevant (rumor) in DB searches for those text fields having a relatively long text, rather than just holding a name.
- Greek words are automatically transliterated, according to the Pinakes convention. This does not mean that Greek is not indexed, but only that for each Greek word, a corresponding transliterated word is added to the index, side to side to the Greek form. The Greek form of course is still subject to all the transformations illustrated above.

### Search Author

Filters:

- name (any portion): `auteurs.nom`[T].
- aliases (as above): `auteurs_alias.nom`[T].
- category (yes/no): true if it's a category, false if it's an author; null if not specified.
- centuryMin, centuryMax: `auteurs.siecle`.

```sql
select
auteurs.id, auteurs.nom, auteurs.siecle, auteurs.dates, auteurs.remarque, auteurs.is_categorie,
aa.nom as alias,
-- keywords are not used, but I left them in this sample
k.keyword as keyword
from auteurs
left join auteurs_alias aa on auteurs.id=aa.id_auteur
left join keywords_auteurs ka ON auteurs.id=ka.id_auteur
left join keywords k ON k.id=ka.id_keyword
limit 10
```

From the resulting list of authors, one can open the full list of works for each author.

### Search Work

Filters:

- subset: this probably would not be exposed in a UI. It filters works according to the value of `equipe_referente` (e.g. `RAP`), so that search is limited even though we have more data.
- title (any portion): `oeuvres.titre`[T].
- curator (any portion): TODO: find field.
- RAP number: `oeuvres.id`.
- manuscript ID (Diktyon): `cote.id`.
- aliases (as above): `oeuvres_alias.titre`[T].
- titulus (as above): `oeuvres.titulus`[T].
- incipit (as above): `oeuvres.incipit`[T].
- desinit (as above): `oeuvres.desinit`[T].
- centuryMin, centuryMax: `oeuvres.siecle`.
- place (as above): `oeuvres.lieu`[T].
- remark: `oeuvres.remarque`[T].
- keyword (0 or more): `keywords.keyword`[T] via `keywords_oeuvres`.
- author (any portion): Zotero
- title (any portion): Zotero
- having any recensions: any, false, true (check if any record exists for the FK `recensions.id_oeuvre`).
- having relation (1 optional relation to pick from the list, or just any to match any type of relation). This can be connected with the next filter. Relation is via table `relations`; types of relations are picked from `relations_types`.
- with work (1 work to pick from a list, or nothing to match any relation target). Field: `relations.id_child`.

The relation filter could then be used to find all the works having some kind of relation to any other work, or to just a specific work; or having some relation to a specific work, either a specific relation or any.

It's not clear how this is connected with the work attributes and their direct, "weak" relation to another work (`oeuvres_attributs`). I could just include this kind of relation in the general filter proposed above, which would thus be able to find both the "weak" and the "strong" relations. If instead this is not used, we can just ignore it.

```sql
select
oeuvres.id, oeuvres.titre, oeuvres.titulus, oeuvres.incipit, oeuvres.desinit, oeuvres.siecle,
oeuvres.date, oeuvres.date_remarque, oeuvres.lieu, oeuvres.lieu_remarque, oeuvres.remarque,
oeuvres.equipe_referente,
oa.titre as alias,
k.keyword as keyword
from oeuvres
left join oeuvres_alias oa on oeuvres.id=oa.id_oeuvre
left join keywords_oeuvres ko ON oeuvres.id=ko.id_oeuvre
left join keywords k ON k.id=ko.id_keyword
where oeuvres.equipe_referente='rap'
limit 10
```

Also, title and incipit should be handled as separate Embix documents to treat them as a single token for search purposes.

### Bibliography

If we are going to search inside bibliography, and combine this search with data from the DB, we will probably have better get the required bibliographic data from Zotero and build a local index; otherwise performance would suffer, as we would have to fire two different searches, where one of them is on external server reached via web, and combine the results. Also, the Zotero server is not designed to handle a lot of search requests, as of course its resources are limited.

So here we should first decide how to use bibliography: if we just have to display it, we can query Zotero. If instead some of its data must become an active part of the search, we must get the subset of data we need, via Zotero API. For instance, the items found at <https://www.zotero.org/groups/44775/manuscripts_on_microfilm_project/library> can be easily retrieved via API calls.

### Embix Profile

The text-based index portion of the search engine uses [Embix](https://github.com/vedph/embix). The Embix profile for Pinakes uses a single document, as defined by this query:

```sql
SELECT
 -- work
 w.id AS m_targetid,
 w.titre AS wkttl,
 w.titulus AS wktit,
 w.incipit AS wkinc,
 w.desinit AS wkdes,
 w.lieu AS wkplc,
 w.remarque AS wkcom,
 -- work aliases
 ws.titre AS wsttl,
 -- work keywords
 k1.keyword AS wkkey,
 -- work's authors with their aliases and keywords
 a.nom AS aunam,
 aa.nom AS aanam,
 k2.keyword AS aukey
FROM oeuvres w
LEFT JOIN oeuvres_alias ws ON ws.id_oeuvre=w.id
LEFT JOIN keywords_oeuvres kw ON kw.id_oeuvre=w.id
LEFT JOIN keywords k1 ON kw.id_keyword=k1.id
LEFT JOIN oeuvres_auteurs wa ON wa.id_oeuvre=w.id
LEFT JOIN auteurs a ON a.id=wa.id_auteur
LEFT JOIN auteurs_alias aa ON aa.id_auteur=a.id
LEFT JOIN keywords_auteurs ka ON ka.id_auteur=a.id
LEFT JOIN keywords k2 ON ka.id_keyword=k2.id
ORDER BY w.id
```

This is just a catch-all query which collects all the text-based sources pointing to the same work, i.e.:

- work's title and its eventual aliases.
- work's titulus, incipit, desinit, place, comment.
- work's keywords.
- work's authors, each with his name, eventual aliases, and keywords.

There are 2 filter chains in this profile:

- `wsp-std` is the chain applied to the text field as a whole, before tokenization. It normalizes whitespaces, and applies a standard filter.
- `stp` is the chain applied after tokenization, to each token. It just includes a stopwords filter.
- the only tokenizer used is the standard whitespace tokenizer, `std`. It has `stp` as its token filters chain.
- some fields, which may have Greek content, add a Greek romanizer token multiplier (`rom`).

### Transliteration

Transliteration uses a Greek token multiplier, which provides the transliterated form of each Greek token next to it. As the transliterated form is targeted to the index process, it comes already deprived of any rumor feature, like accents or casing; this is done by just using a 7-bit ASCII encoding as the transliteration target.

These are the Pinakes conventions:

- `β` = `b`
- `ε` = `e`
- `ζ` = `z`
- `η` = `e`
- `θ` = `th`
- `κ` = `k`
- `ξ` = `x`
- `ρ` = `rh`/`r` (`rh` en début de mot ou lorsqu’il a deux ρ, sinon simplement `r`)
- `υ` = `y`
- `φ` = `ph`
- `χ` = `ch`
- `ω` = `o`
- `γγ` = `gg`
- `ευ` = `eu`
- `μπ` = `mp`
- `ου` = `ou`
- esprit rude = `h`
