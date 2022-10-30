from urlextract import URLExtract

extractor = URLExtract()
i = " g https://www.hui.ru afsd"
urls = extractor.find_urls(i)
print(urls[0])
print("http" in i)