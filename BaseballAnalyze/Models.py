import pandas as pd
import plotly.express as px

pitch = {'FF':'4-seam', 'SI':'Sinker', 'FC':'Cutter', 'CH':'Changeup', 'FS':'Split', 'FO':'Fork',
         'SC':'Screw', 'CU':'Curve', 'KC':'KnuckleCurve', 'CS':'SlowCurve', 'SL':'Slider',
         'ST':'Sweeper', 'SV':'Slurve', 'KN':'KnuckleBall', 'EP':'Eephus', 'IN':'InternationalBall',
         'PO':'Pitchput'}
def readData(fileName):
    df = pd.read_csv(f'{fileName}', encoding='utf-8')
    return df

def getMonth(datetime: str):
    # '2022-08-02' -> 8
    dateString = datetime.split('/')
    return int(dateString[1])

def deleteTime(datetime: str):
    # '2022-01-01 0:00:00' -> '2022-01-01'
    dateString = datetime.split(' ')
    return dateString[0]

def formatDatetime(dataframe):
    dataframe['gameDate'] = pd.to_datetime(dataframe['gameDate'], format='%Y/%m/%d')
    return dataframe

def processPitch(df):
    df['pitch'] = df['pitch'].map(pitch)
    # print(df['pitch'])
    return df
def processDate(df):
    df['gameDate'] = df['gameDate'].apply(deleteTime)
    df['Month']=df['gameDate'].apply(getMonth)
    formatDatetime(df)
    return formatDatetime(df)

def processData(df):
    df = processPitch(df)
    df = processDate(df)
    return df
def compute_mph_spin(df):
    df = df[['pitch', 'mph', 'spinRate', 'Month']]
    # get each pitch per month -> df:count
    count = df.groupby(['pitch','Month'], as_index=False).count()
    count['count'] = count['mph']
    count = count[['pitch', 'Month', 'count']]

    # compute mean of mph and spinrate -> df: result
    result = df.groupby(['pitch','Month'], as_index=False).mean().round(2)

    # join result and count
    result = result.join(count.set_index(['pitch', 'Month']), on=['pitch', 'Month'])
    result = result.fillna(0)
    result.to_csv('result.csv', encoding='utf-8-sig')
    return result

def plot_mph(df):
    lower = df['mph'].min()
    upper = df['mph'].max()
    fig = px.line(df, x='Month', y='mph', animation_frame='pitch', text='mph',
                 range_y=[lower-1, upper+5])
    fig.update_traces(textposition="top center")
    fig.show()

def plot_spinrate(df):
    lower = df['spinRate'].min()
    upper = df['spinRate'].max()
    fig2 = px.line(df, x='Month', y='spinRate', animation_frame='pitch', text='spinRate', range_y=[lower-1, upper+100])
    fig2.update_traces(textposition="top center")
    fig2.show()

def plot_pitchCount(df):
    lower = df['count'].min()
    upper = df['count'].max()
    fig2 = px.bar(df, x='pitch', y='count', animation_frame='Month', text='count', range_y=[lower-5, upper+5])
    fig2.update_traces(textposition="outside")
    fig2.show()

def mainFunction(fileName):
    df = processData(readData(fileName))
    result = compute_mph_spin(df)
    plot_mph(result)
    plot_spinrate(result)
    plot_pitchCount(result)

if __name__ == "__main__":
    mainFunction()

