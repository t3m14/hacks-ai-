import pandas as pd
import requests
from bs4 import BeautifulSoup
import asyncio
from urlextract import URLExtract
from lemmatizer import lemmatize
import csv


links_list = []
extractor = URLExtract()
all_text = ""
keywords = []

def write_data(url, keywords):
    w_file = open('test2.csv', 'a', encoding="utf8")
    file_writer = csv.writer(w_file, delimiter=' ')
    print(url)
    print(keywords)
    if keywords:
        file_writer.writerow([url, keywords])

links_list = []
extractor = URLExtract()
all_text = ""
keywords = []
def lem(link):
    headers = {'Accept-Encoding': 'identity'}
    html = requests.get(link, headers=headers)
    html.encoding = html.apparent_encoding
    html = html.text
    
    soup = BeautifulSoup(html, "html.parser")
    global all_text
    ps = soup.findAll("p")
    spans = soup.findAll("span")
    divs = soup.findAll("div")
    for div in divs:
        div_text = div.text
        all_text += div_text 
    for span in spans:
        span_text = span.text
        all_text += span_text 
    for p in ps:
        p_text = p.text
        all_text += p_text 
    print("ССЫЛКА:\n")
    print(f"\n\n{link}\n\n")
    print("САМЫЕ ЧАСТЫЕ СЛОВА:\n")
    print(f"\n\n{lemmatize(all_text, 10)}\n\n")
    # print(all_text)
    global keywords
    keywords = lemmatize(all_text, 20)
counter = 0

async def find_all_urls(url):
    await asyncio.sleep(0)
    local_links = []
    global counter
    headers = {'Accept-Encoding': 'identity'}
    html = requests.get(url, headers=headers)
    html.encoding = html.apparent_encoding
    html = html.text
    
    soup = BeautifulSoup(html, 'html.parser')
    all_a = soup.findAll("a")
    for a in all_a:
        if counter < 5:
            counter += 1
            link = a.get("href")
            if str(link).startswith("/"):
                if url[-1] == '/' and link[-1] == '/':
                    url = url.replace("/", "")
                link = url+link
                global links_list
                if link.split(".")[-1] != "pdf":
                    if link not in links_list:
                        links_list.append(link)
                    if link not in local_links:
                        local_links.append(link)
                        lem(link)
        else:pass
        
    counter = 0
    global all_text
    global keywords
    write_data(url, keywords)
    keywords = []
    all_text = ""
def get_urls_list():
    xls = pd.ExcelFile(r"./1. Компании.xlsx")
    sheetX = xls.parse(0)
    links = []
    for i in sheetX["Сайт"]:
        urls = extractor.find_urls(i)
        if urls:
            link = urls[0]
            if "http" not in i: 
                links.append("http://" + str(link)) if link not in links and str(link) != "nan" else i
            elif "http" in i:
                links.append(str(link)) if link not in links and str(link) != "nan" else i
    return links

# def main():
#     all_links = get_urls_list()
#     for link in all_links:
#         find_all_urls(url=link)
# main()
async def main():
    all_links = get_urls_list()
    tasks = []
    for link in all_links:
        task = asyncio.create_task(find_all_urls(url=link))
        tasks.append(task)
    await asyncio.gather(*tasks)

asyncio.run(main())
