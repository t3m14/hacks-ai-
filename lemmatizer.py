import pymorphy2
import re
from collections import defaultdict
import spacy
# Лемматизация слов из текста и поиск самых частых слов

patterns = "[A-Za-z0-9!#$%&'()*+,./:;<=>?@[\]^_`{|}~—\"\-]+"
morph = pymorphy2.MorphAnalyzer()
def lemmatize(doc, needed_words):
    word_freq = defaultdict(int)
    doc = re.sub(patterns, ' ', doc)
    tokens = []
    for token in doc.split():
        p = morph.parse(token)[0]
        if "NOUN" in p.tag or "VERB" in p.tag:
            normal = p.normal_form
            word_freq[normal] += 1
    return sorted(word_freq, key=word_freq.get, reverse=True)[:needed_words]
# s = """
# Челябинск ул. вернадский пер. д. 12 я там учусь и дрочусь
# """
# print(lemmatize(s, 20))