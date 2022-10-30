import pymorphy2
import re
from collections import defaultdict
import pandas as pd
from gensim.models import Word2Vec
from pandas import ExcelWriter

patterns = "[A-Za-z0-9!#$%&'()*+,./:;<=>?@[\]^_`{|}~—\"\-]+"
morph = pymorphy2.MorphAnalyzer()
def lemmatize(doc, needed_words=10):
    word_freq = defaultdict(int)
    doc = re.sub(patterns, ' ', doc)
    tokens = []
    for token in doc.split():
        p = morph.parse(token)[0]
        if "NOUN" in p.tag or "VERB" in p.tag:
            normal = p.normal_form
            word_freq[normal] += 1
    return sorted(word_freq, key=word_freq.get, reverse=True)[:needed_words]
def color(value):
    if value < 0.3:
        return ['background-color: yellow']
def find_discrepancy():
    print("Скачивание датафреймов")

    df1 = pd.read_excel("1. Компании.xlsx")
    parse_df = pd.read_csv("test2.csv")
    train_res = df1.merge(parse_df, on=["Сайт"])
    industry_df = pd.read_excel("Справочник. Отрасли и подотрасли.xlsx")
    tech_df = pd.read_excel("Справочник. Технологии.xlsx")
    print("Датафреймы скачаны")
    print("Лемматизация...")
    train_res["Описание компании"] = train_res["Описание компании"].apply(lemmatize)
    tech_df["3 уровень (уровень тегирования участников)"] = tech_df["3 уровень (уровень тегирования участников)"].apply(lemmatize)
    industry_df["Наименование подотрасли"] = industry_df["Наименование подотрасли"].apply(lemmatize)
    print("Лемматизация окончена")
    model = Word2Vec(vector_size=100, window=5, min_count=1, workers=4)
    model.save("word2vec.model")
    model = Word2Vec.load("word2vec.model")
    to_learn = train_res["Описание компании"].append(train_res["Лемма"]).append(industry_df["Наименование подотрасли"]).append(tech_df["3 уровень (уровень тегирования участников)"])
    model.build_vocab(to_learn)
    print("Словарь собран")
    print("Определяем схожесть...")
    train_res["Схожесть"] = train_res.apply(lambda row: model.wv.n_similarity(row["Описание компании"], row["Лемма"]), axis = 1)
    writer = ExcelWriter('PythonExport.xlsx')
    train_res.to_excel(writer,'output')
    writer.save()
    print("Вывод в PythonExport.xlsx")
find_discrepancy()
